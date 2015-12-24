using System;
using System.Linq;
using System.Numerics;

namespace CapdEmulator.Devices
{
  interface ISignalModuleContext
  {
    int Frequency { get; }
    /// <summary>
    /// Количество каналов в модуле, чтобы знать, 
    /// сколько генерировать сигналов за единицу времени.
    /// </summary>
    byte ChannelCount { get; }
    double GetParameter(int id);
  }

  abstract class SignalGeneratorBase : ISignalGenerator
  {
    protected ISignalModuleContext ModuleContext { get; private set; }

    /// <summary>
    /// Результирующие текущие данные каналов. Для возврата из метода Calculate().
    /// В целях оптимизации выделим память по количеству каналов один раз
    /// и данный массив будем все время возращать из метода Calculate()
    /// в наследниках меняя данные.
    /// </summary>
    protected int[] ResultantSignals { get; private set; }

    public SignalGeneratorBase(ISignalModuleContext moduleContext)
    {
      ModuleContext = moduleContext;
      ResultantSignals = new int[ModuleContext.ChannelCount];
    }

    #region ISignalGenerator

    public virtual void Prepare()
    {
      // Если количество каналов изменилось, переинициализируем буфер.
      if (ResultantSignals.Length != ModuleContext.ChannelCount)
      {
        ResultantSignals = new int[ModuleContext.ChannelCount];
      }
    }

    public int Frequency
    {
      get { return ModuleContext.Frequency; }
    }

    public abstract int[] Calculate(int timePoint);

    // Здесь только выполнения команд с целью управлением сигналом.
    // Ответ внешней программе должен посылать сам модуль.
    public abstract void Execute(Command command);

    #endregion
  }

  interface IPulseVisualContext
  {
    int Pulse { get; }
  }

  class PulseSinusGenerator : SignalGeneratorBase
  {
    private IPulseVisualContext visualContext;

    public PulseSinusGenerator(ISignalModuleContext moduleContext, IPulseVisualContext visualContext) 
      : base(moduleContext) 
    {
      this.visualContext = visualContext;
    }

    #region SignalGeneratorBase

    public override int[] Calculate(int timePoint)
    {
      double k = visualContext.Pulse / 60.0;
      ResultantSignals[0] = (int)(Math.Sin(2 * Math.PI / Frequency * k * timePoint) * 100000);
      return ResultantSignals;
    }

    public override void Execute(Command command)
    {
      // Здесь только выполнения команд с целью управлением сигналом.
      // Ответ внешней программе должен посылать сам модуль.
    }

    #endregion
  }

  interface IPressVisualContext
  {
    int Sistol { get; }
    int Diastol { get; }

    double Press { set; }
  }

  class PressSinusGenerator : SignalGeneratorBase
  {
    IPressVisualContext visualContext;

    /// <summary>
    /// Текущее значение сигнала давления.
    /// </summary>
    int currentValue;
    /// <summary>
    /// Текущее состояние уменьшения давления.
    /// </summary>
    int currentDecrease;
    /// <summary>
    /// Текущее состояние увеличения давления.
    /// </summary>
    int currentIncrease;

    /// <summary>
    /// Уменьшение давления при открытом клапане.
    /// </summary>
    int decreaseForGateOff = 1000;
    /// <summary>
    /// Уменьшение давления при закрытом клапане.
    /// </summary>
    int decreaseForGateOn = 60;
    /// <summary>
    /// Увеличение давления при неработающем компрессоре.
    /// </summary>
    int increaseForPumpOff = 0;
    /// <summary>
    /// Увеличение давления при работающем компрессоре.
    /// </summary>
    int increaseForPumpOn = 400;

    double? gain;

    public PressSinusGenerator(ISignalModuleContext moduleContext, IPressVisualContext visualContext)
      : base(moduleContext)
    {
      this.visualContext = visualContext;

      // Проинициализируем внутреннее состояние.
      currentValue = 0;
      currentDecrease = decreaseForGateOff;
      currentIncrease = increaseForPumpOff;
    }

    #region SignalGeneratorBase

    public override int[] Calculate(int timePoint)
    {
      if (!gain.HasValue)
      {
        // Расчитаем коэфициент усиления.
        double par1 = ModuleContext.GetParameter(1);
        double par2 = ModuleContext.GetParameter(2);
        double par11 = ModuleContext.GetParameter(11);
        double par51 = ModuleContext.GetParameter(51);
        double adcCapacity = Math.Pow(2, par1);
        gain = 7.5 * par2 / adcCapacity / par11 / par51;
      }

      // Постоянная составляющая давления.
      currentValue = currentValue + currentIncrease - currentDecrease;
      currentValue = Math.Max(0, currentValue);
      currentValue = (int)Math.Min(300 / gain.Value, currentValue);
      double press = currentValue * gain.Value;

      // Если это первый заход или пошла новая секунда,
      // то оповестим визуальный контекст о текущем давлении.
      if (timePoint == 0 || (timePoint - 1) / Frequency < timePoint / Frequency)
      {
        visualContext.Press = press;
      }

      // Если постоянная составляющая давления попадает в интервал
      // установленного давления, добавим сигнал синуса.
      double ampl;
      if (press >= visualContext.Diastol && press <= visualContext.Sistol)
      {
        double koef = Math.Sin(Math.PI / (visualContext.Sistol - visualContext.Diastol) * (visualContext.Sistol - press));
        ampl = 50 + koef * 950;
      }
      else if (press > 5) // При давлении больше 5, начнем подавать ЧСС.
      {
        ampl = 50;
      }
      else
      {
        ampl = 0;
      }

      currentValue += (int)(Math.Sin(2 * Math.PI / Frequency * timePoint) * (ampl));

      ResultantSignals[0] = currentValue;
      return ResultantSignals;
    }

    public override void Execute(Command command)
    {
      // Здесь только выполнения команд с целью управлением сигналом.
      // Ответ внешней программе должен посылать сам модуль.
      switch (command)
      {
        case Command.opGateOff:
          currentDecrease = decreaseForGateOff;
          break;
        case Command.opGateOn:
          currentDecrease = decreaseForGateOn;
          break;
        case Command.opPumpOff:
          currentIncrease = increaseForPumpOff;
          break;
        case Command.opPumpOn:
          currentIncrease = increaseForPumpOn;
          break;
      }
    }

    #endregion
  }

  class EcgFourierGenerator : SignalGeneratorBase
  {
    #region Спектр сигнала ЭКГ для частоты 1116.

    int ecgFrequency = 1116;
    int ecgPeriod = 700;
    Complex[][] ecgSpectors = new Complex[][]
    {
      new Complex[]
      {
        new Complex(-2813917, 0),
        new Complex(62104.5942168841, 36989.9567642004),
        new Complex(-79802.8890838774, -41919.8577610809),
        new Complex(5588.47326998943, 9995.67477156228),
        new Complex(-37175.6883966784, -61781.4817668995),
        new Complex(3729.68559915433, 80066.7322477829),
        new Complex(40638.8206954163, -43382.7484605055),
        new Complex(-47180.0643804005, 51749.4259279559),
        new Complex(75938.8802256604, -20803.953812073),
        new Complex(-73063.4188541417, -23438.162155033),
        new Complex(51309.6461549093, 44217.9386932472),
        new Complex(-28535.1090643051, -64111.9858397227),
        new Complex(-3044.66603942338, 62991.3812086644),
        new Complex(29694.5562413763, -52152.8416957311),
        new Complex(-45028.1625530465, 29354.3767094336),
        new Complex(43581.8657563705, -4691.56960318483),
        new Complex(-39006.8702124824, -8949.11139456045),
        new Complex(24255.7921389562, 21345.2918238616),
        new Complex(-9550.69013682294, -24903.5698675698),
        new Complex(1170.13807913358, 21425.5125240533),
        new Complex(4891.35714364038, -16350.7224350013),
        new Complex(-6648.73120343606, 11695.6863893458),
        new Complex(7507.71928809525, -7176.10001428916)
      },
      new Complex[]
      {
        new Complex(-2486472, 0),
        new Complex(87334.2577130414, 113720.860600566),
        new Complex(-158769.770409537, -42834.8724195429),
        new Complex(15817.8810111651, -2395.77685496813),
        new Complex(-50026.5789042307, -113069.98195877),
        new Complex(32671.6418589097, 122327.63026568),
        new Complex(57014.8921157965, -79553.8719625926),
        new Complex(-83547.4702088863, 102563.638755602),
        new Complex(120410.783928252, -46683.8767031511),
        new Complex(-132994.863388536, -28621.6969865505),
        new Complex(101557.924620113, 71531.0008475323),
        new Complex(-57098.7074244035, -114398.323242569),
        new Complex(685.741423728458, 116687.513327801),
        new Complex(53388.3764006209, -96555.0242618267),
        new Complex(-81533.6252560518, 56964.0668025076),
        new Complex(80128.6262710677, -9755.77082982604),
        new Complex(-68617.8911045491, -22734.9562786715),
        new Complex(43803.0074696189, 41571.3381544065),
        new Complex(-17770.3247541333, -43838.8890189766),
        new Complex(1283.13313312464, 35620.8277561607),
        new Complex(9223.06721208716, -26393.8351072756),
        new Complex(-12689.5218341254, 18390.0422803278),
        new Complex(13706.9030433385, -12113.1475477368)
      },
      new Complex[]
      {
        new Complex(-2728394, 0),
        new Complex(9309.64016951882, 113516.501067563),
        new Complex(-163617.512774638, 48979.7918029977),
        new Complex(635.068356528017, -137278.789914206),
        new Complex(-109952.265908553, -49772.3593308749),
        new Complex(130909.687698882, 98935.7651948357),
        new Complex(-12932.6271752453, -81493.8042475911),
        new Complex(-31426.8288848248, 164267.420423839),
        new Complex(107972.299914458, -112673.842523244),
        new Complex(-159570.408702635, 18496.5191160568),
        new Complex(142051.76077174, 45323.521888503),
        new Complex(-93685.5949503885, -120384.152466851),
        new Complex(23134.1375287429, 136601.073739544),
        new Complex(52229.6273730982, -120056.876221713),
        new Complex(-94563.3389612575, 70457.1132813083),
        new Complex(91283.3721242863, -7840.9892063438),
        new Complex(-76987.2680541363, -29822.6457570404),
        new Complex(41795.6137588001, 51136.8742252402),
        new Complex(-8574.65673934894, -48422.3962962018),
        new Complex(-6438.35311361936, 33547.5071848765),
        new Complex(14721.9671175631, -18878.076380597),
        new Complex(-13440.0555265542, 13125.2478695615),
        new Complex(11030.1936922335, -7020.69386041595)
      },
      new Complex[]
      {
        new Complex(-2456721, 0),
        new Complex(-67094.7865021039, 42014.819503879),
        new Complex(-91966.6383444461, 112412.272658409),
        new Complex(-5646.46131420911, -205375.877623892),
        new Complex(-140513.640458397, 31932.5445176774),
        new Complex(176680.127367115, 51708.1240258064),
        new Complex(-80342.6769826721, -68573.5619267099),
        new Complex(29253.4668322216, 174130.167327644),
        new Complex(70879.7283958281, -148196.062136443),
        new Complex(-141674.250593891, 61967.4694198021),
        new Complex(144658.341395997, 11073.612707615),
        new Complex(-108386.750565848, -95197.2375595349),
        new Complex(38417.0940636903, 124610.545073878),
        new Complex(37757.7792894925, -114061.555056291),
        new Complex(-84330.8354457153, 67850.3479015668),
        new Complex(83118.3408168045, -4880.99641814603),
        new Complex(-65660.530479985, -31075.6553983268),
        new Complex(28455.8020809976, 49991.3703687884),
        new Complex(3440.01153408046, -42097.6826274677),
        new Complex(-14590.644240628, 24034.5755045264),
        new Complex(16850.0230808792, -7791.75085386691),
        new Complex(-10036.8583336714, 2549.78517952678),
        new Complex(5623.55268295563, 338.799752320885)
      },
      new Complex[]
      {
        new Complex(-2566668, 0),
        new Complex(-80761.2437602655, 9892.53533883997),
        new Complex(-51338.2787430681, 107503.091870374),
        new Complex(-14910.9306081409, -196986.183644324),
        new Complex(-131614.332109319, 60363.2026898462),
        new Complex(164086.722509414, 23846.5284027767),
        new Complex(-94791.3670103212, -46492.0577367543),
        new Complex(55843.1396165292, 150850.365839521),
        new Complex(41601.4197425053, -142250.255799084),
        new Complex(-112584.655632713, 71622.9307864376),
        new Complex(126593.973819649, -7947.70054424106),
        new Complex(-101108.222175144, -69310.8681207512),
        new Complex(41874.8020252642, 102827.303092067),
        new Complex(25862.1486603889, -98119.7895174432),
        new Complex(-70312.566041129, 57905.3202623228),
        new Complex(68411.6678898217, -3756.70705443266),
        new Complex(-53533.3861818033, -28438.2323619103),
        new Complex(21273.058663835, 43751.5350666601),
        new Complex(7421.1404878138, -34289.8933240304),
        new Complex(-16427.1661057371, 16503.4067683876),
        new Complex(15910.7018767502, -1327.37666799884),
        new Complex(-7031.53160853193, -2392.36321710274),
        new Complex(2359.4258881351, 2926.48861190252)
      },
      new Complex[]
      {
        new Complex(-1322972, 0),
        new Complex(-60449.3781783833, 15962.8329146663),
        new Complex(-40210.715054345, 82843.7467126879),
        new Complex(-13838.4251842983, -147027.882095559),
        new Complex(-89306.4547399352, 44418.7581869194),
        new Complex(120665.491890955, 11567.0324108532),
        new Complex(-68103.408411714, -30855.6335451544),
        new Complex(41499.2915357992, 108578.968816484),
        new Complex(24075.4250064944, -103089.381278536),
        new Complex(-78437.5961697687, 52397.1298376975),
        new Complex(89371.9904257125, -9497.45370637174),
        new Complex(-74899.0772775924, -46954.9371571845),
        new Complex(32328.266679583, 71358.7866742261),
        new Complex(17397.4225715536, -69471.7529043417),
        new Complex(-49019.7357645915, 42417.6600065682),
        new Complex(49630.3164689677, -2875.60198615943),
        new Complex(-39144.6666022809, -19416.1369545376),
        new Complex(15392.0299732664, 29594.6375229387),
        new Complex(5722.68949846213, -23757.3940727724),
        new Complex(-12861.4467732934, 11534.7278973958),
        new Complex(10677.7602641871, -542.753753077105),
        new Complex(-4993.10403300996, -1867.84392862962),
        new Complex(843.074182697178, 2669.91385095301)
      },
      new Complex[]
      {
        new Complex(-1828732, 0),
        new Complex(41746.2261752947, -3227.70396993673),
        new Complex(26954.83681163, -58520.0402570958),
        new Complex(8109.52185565765, 104892.889200061),
        new Complex(65851.4974695981, -32316.3974580347),
        new Complex(-83072.3248437545, -11511.6984497084),
        new Complex(47160.3631895261, 21055.7686618943),
        new Complex(-27853.9831190705, -73485.2268960966),
        new Complex(-21534.3667993711, 68911.25307071),
        new Complex(54493.9417977091, -32763.72447037),
        new Complex(-60338.0033463148, 4047.24581180719),
        new Complex(47028.6659718681, 33548.1697585734),
        new Complex(-17963.5763734762, -48841.219735056),
        new Complex(-14744.1536371086, 45102.2811542506),
        new Complex(33299.7438231155, -24500.4213504147),
        new Complex(-31408.035185221, -1303.61256478328),
        new Complex(22939.8755913092, 14614.0213339231),
        new Complex(-6631.29117218562, -21476.5685029583),
        new Complex(-5799.16860607391, 15842.3632567484),
        new Complex(8391.59257315074, -6361.45621574793),
        new Complex(-7491.66007282923, -576.410990454428),
        new Complex(3152.27615602036, 2150.55062677925),
        new Complex(-679.654286964576, -1558.07753207766)
      },
      new Complex[]
      {
        new Complex(-3050707, 0),
        new Complex(-14420.0652749792, -445.657501736123),
        new Complex(-537.025639436469, 17893.3394828659),
        new Complex(-2467.64793464858, -26678.5240177088),
        new Complex(-13007.6758933363, 12315.9619512202),
        new Complex(20406.0545465107, -2385.94875251407),
        new Complex(-15487.7984252642, -2433.59969730172),
        new Complex(11269.123940761, 15033.4823924979),
        new Complex(769.637382039479, -16483.3883785959),
        new Complex(-9841.07657983532, 10293.8579677203),
        new Complex(12970.0108891639, -4328.26797616817),
        new Complex(-11084.8076364432, -4808.60353240786),
        new Complex(4794.67871137735, 8532.79758521627),
        new Complex(2630.76390700576, -7677.01355384777),
        new Complex(-6426.269840253, 5660.24892603406),
        new Complex(6053.18268460975, 116.173846006795),
        new Complex(-4416.90997670731, -3650.59518275536),
        new Complex(607.204366529735, 4842.56732989822),
        new Complex(2285.62325036313, -2213.53993004195),
        new Complex(-1890.98433667007, -671.652755927628),
        new Complex(808.856959648741, 1374.06866715257),
        new Complex(-16.4275195609107, -968.816554329546),
        new Complex(376.531488193438, 1157.03254496881)
      },
      new Complex[]
      {
        new Complex(-4973729, 0),
        new Complex(-27353.8658670716, 7081.02021957414),
        new Complex(-26847.3578364021, 40471.4480842146),
        new Complex(-6174.04365584211, -79564.4729086742),
        new Complex(-52619.2647367709, 18756.9548501592),
        new Complex(62208.0645430742, 15987.7590712937),
        new Complex(-30990.9735378326, -18590.1719447888),
        new Complex(16700.2996582497, 59582.0530525393),
        new Complex(21972.0417774391, -51848.977890984),
        new Complex(-46038.9050329689, 21668.5337451218),
        new Complex(47070.9532396301, 1164.21901322576),
        new Complex(-34312.301622238, -30459.9440685463),
        new Complex(10228.6758050873, 39698.9616730551),
        new Complex(15164.842154272, -35293.4576224334),
        new Complex(-28866.8757220166, 19153.5758253784),
        new Complex(25564.7444464274, 980.178846525225),
        new Complex(-18669.0721771231, -12158.3962312696),
        new Complex(5897.14598405532, 17131.1165911092),
        new Complex(4157.69372022888, -12903.4568740614),
        new Complex(-6589.60721805595, 5288.55184369149),
        new Complex(5722.82040418746, 381.851110977706),
        new Complex(-2906.38828381258, -695.663593377275),
        new Complex(767.261289657768, 1066.99604374374)
      }
    };

    #endregion

    int calcCouter = 0;
    private double[][] signals;

    public EcgFourierGenerator(ISignalModuleContext moduleContext) : base(moduleContext) { }

    public override void Prepare()
    {
      base.Prepare();
#warning При частоте 2232 канал WCF не справляется с нагрузкой. Также здесь не делается передискретизация на более высокую частоту.

      // Получаем сигнал для частоты ecgFrequency (восстанавливаем).
      signals = new double[ecgFrequency][];
      for (int i = 0; i < ecgSpectors.Length; i++)
      {
        var signal = DFT.InverseFourierTransform(ecgSpectors[i], ecgPeriod).Select(v => v.Real);
        // Дополним секунду прямой соединяющей последнюю точку текущего сигнала с первой точкой последующего сигнала.
        double k = (signal.Last() - signal.First()) / (ecgFrequency - ecgPeriod);
        double last = signal.Last();
        var signalNull = Enumerable.Range(0, ecgFrequency - ecgPeriod).Select(j => k * j + last);
        signals[i] = signal.Concat(signalNull).ToArray();
      }

      // Есил установленная частота меньше ecgFrequency, то передискретизируем.
#warning При малой частоте уменьшить передискретизировать.
      if (ModuleContext.Frequency < ecgFrequency)
      {
        double k = (double)ModuleContext.Frequency / ecgFrequency;
        var n = Enumerable.Range(0, ModuleContext.Frequency).Select(v => new { Original = v, New = v * k }).ToArray();
      }
    }

    #region SignalGeneratorBase

    public override int[] Calculate(int timePoint)
    {
      for (int i = 0; i < ResultantSignals.Length; i++)
      {
        ResultantSignals[i] = (int)signals[i][calcCouter];
      }
      calcCouter++;
      calcCouter %= signals[0].Length;
      return ResultantSignals;
    }

    public override void Execute(Command command) { }

    #endregion
  }

  class NullSignalGenerator : SignalGeneratorBase
  {
    public NullSignalGenerator(ISignalModuleContext moduleContext) : base(moduleContext) { }

    public override int[] Calculate(int timePoint)
    {
      return ResultantSignals;
    }

    public override void Execute(Command command) { }
  }

  class SignalGeneratorFactory : ISignalGeneratorFactory
  {
    IPressVisualContext pressVisualContext;
    IPulseVisualContext pulseVisualContext;

    public SignalGeneratorFactory(IPressVisualContext pressVisualContext, IPulseVisualContext pulseVisualContext)
    {
      this.pressVisualContext = pressVisualContext;
      this.pulseVisualContext = pulseVisualContext;
    }

    #region ISignalGeneratorFactory

    public ISignalGenerator Create(ModuleType moduleType, ISignalModuleContext moduleContext)
    {
      switch (moduleType)
      {
        case ModuleType.Pulse:
          return new PulseSinusGenerator(moduleContext, pulseVisualContext);
        case ModuleType.Press:
          return new PressSinusGenerator(moduleContext, pressVisualContext);
        case ModuleType.Ecg:
          return new EcgFourierGenerator(moduleContext);
        default:
          return new NullSignalGenerator(moduleContext);
      }
    }

    #endregion
  }
}
