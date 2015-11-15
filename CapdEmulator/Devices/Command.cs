using System;

namespace CapdEmulator.Devices
{
  enum Command
  {
    /// <summary>
    /// Изменен уровень ЦАП
    /// </summary>
    msgDACOps,
    /// <summary>
    /// Перегрузка канала АЦП
    /// </summary>
    msgADCOverflow,
    /// <summary>
    /// Задана частота АЦП
    /// </summary>
    msgADCFreqSet,
    /// <summary>
    /// Не прошла команда после 3-х попыток
    /// </summary>
    msgCmdNotPass,
    /// <summary>
    /// Включить компрессор
    /// </summary>
    opPumpOn,
    /// <summary>
    /// Выключить компрессор
    /// </summary>
    opPumpOff,
    /// <summary>
    /// Сброс компаратора
    /// </summary>
    opCmpReset,
    /// <summary>
    /// Включить клапан
    /// </summary>
    opGateOn,
    /// <summary>
    /// Выключить клапан
    /// </summary>
    opGateOff,
    /// <summary>
    /// Включить коммутатор (пост+перем)
    /// </summary>
    opMux00,
    /// <summary>
    /// Включить коммутатор (пост+перем)
    /// </summary>
    opMux01,
    /// <summary>
    /// Включить коммутатор (пост+перем)
    /// </summary>
    opMux10,
    /// <summary>
    /// Включить коммутатор (пост+перем)
    /// </summary>
    opMux11,
    /// <summary>
    /// Опрос кнопки
    /// </summary>
    opCheckPBtn,
    /// <summary>
    /// Включить красный излучатель
    /// </summary>
    opPulsRedOn,
    /// <summary>
    /// Включить инфракрасный излучатель
    /// </summary>
    opPulsIRedOn,
    /// <summary>
    /// Выключить излучатель нулями
    /// </summary>
    opPulsOff0,
    /// <summary>
    /// Выключить излучатель единицами
    /// </summary>
    opPulsOff1,
    /// <summary>
    /// Сбросить АЦП
    /// </summary>
    opADCReset,
    opComReset,
    opCOM_IN_IIC_CHECK,
    /// <summary>
    /// Нажата кнопка (компрессор включен)
    /// </summary>
    msgPBtnPress,
    /// <summary>
    /// Отжата кнопка (компрессор отключен)
    /// </summary>
    msgPBtnDepress,
    /// <summary>
    /// Клапан включен
    /// </summary>
    msgGateOn,
    /// <summary>
    /// Клапан выключен
    /// </summary>
    msgGateOff,
    /// <summary>
    /// Насос включен
    /// </summary>
    msgPumpOn,
    /// <summary>
    /// Насос выключен
    /// </summary>
    msgPumpOff,
    /// <summary>
    /// Компаратор сработал
    /// </summary>
    msgCmpOver,
    /// <summary>
    /// Компаратор сброшен
    /// </summary>
    msgCmpReset,
    /// <summary>
    /// Коммутатор включен (пост+перем) (Для блока давления с АЦП - включение алгоритма)
    /// </summary>
    msgMuxOn00,
    /// <summary>
    /// Коммутатор включен (пост+перем от gnd) (Для блока давления с АЦП - выключение алгоритма)
    /// </summary>
    msgMuxOn01,
    /// <summary>
    /// Коммутатор включен (перегрузка)
    /// </summary>
    msgMuxOn10,
    /// <summary>
    /// Коммутатор включен (переменный)
    /// </summary>
    msgMuxOn11,
    /// <summary>
    /// Включен красный излучатель
    /// </summary>
    msgRedBeamOn,
    /// <summary>
    /// Включен инфракрасный излучатель
    /// </summary>
    msgIRBeamOn,
    /// <summary>
    /// Выключены излучатели нулями
    /// </summary>
    msgBeamsOff0,
    /// <summary>
    /// Выключены излучатели единицами
    /// </summary>
    msgBeamsOff1,
    /// <summary>
    /// Потеря отсчетов в канале
    /// </summary>
    msgLossSamples,
    /// <summary>
    /// АЦП запущен
    /// </summary>
    msgADCStart,
    /// <summary>
    /// АЦП остановлен
    /// </summary>
    msgADCStop,
    /// <summary>
    /// Не приходят данные из блока (таймаут 5 сек)
    /// </summary>
    msgBadStopUSB,
    /// <summary>
    /// Потеря синхронизации потока данных
    /// </summary>
    msgLostSynchro,
    /// <summary>
    /// Нештатное прекращение потока отсчетов (таймаут 1 сек)
    /// </summary>
    msgBadStopADC,
    /// <summary>
    /// Установить ШИМ компрессора (целое число байт)
    /// </summary>
    opShimPump,
    /// <summary>
    /// Установить ШИМ клапана (целое число байт)
    /// </summary>
    opShimValve,
    /// <summary>
    /// Уведомление об установке ШИМ компрессора
    /// </summary>
    msgShimPump,
    /// <summary>
    /// Выставляет усиление
    /// </summary>
    opSendPulseGain,
    /// <summary>
    /// Подтверждение о выставлени уиления
    /// </summary>
    msgSendPulseGain,
    /// <summary>
    /// Включаем/выключаем автоусиление
    /// </summary>
    opPulseAutoGainMode
  }
}
