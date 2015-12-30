using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

using CapdxxTester.Models;
using CapdEmulator.WpfUtility;

namespace CapdxxTester.ViewModels
{
  class DeviceViewModel : ChangeableObject
  {
    IDevice device;

    public DeviceViewModel(IDevice device)
    {
      this.device = device;
      device.PropertyChanged += (s, e) => NotifyPropertyChanged(e.PropertyName);

      ModuleViewModels = new ObservableCollection<ModuleViewModel>();
      device.Modules.CollectionChanged += (s, e) =>
      {
        if (e.Action == NotifyCollectionChangedAction.Add)
        {
          foreach (IModule module in e.NewItems.OfType<IModule>())
          {
            if (module.ModuleType == 3 && module is IPressModule)
              ModuleViewModels.Add(new PressModuleViewModel(module as IPressModule));
            if (module.ModuleType == 4)
              ModuleViewModels.Add(new PulseModuleViewModel(module));
            if (module.ModuleType == 5)
              ModuleViewModels.Add(new EcgModuleViewModel(module));
          }
        }
      };
    }

    public string Description { get { return device.Description; } }
    public byte Version { get { return device.Version; } }
    public bool Active { get { return device.Active; } }
    public ObservableCollection<ModuleViewModel> ModuleViewModels { get; private set; }
    public ModuleViewModel SelectedModuleViewModel { get; set; }

    public void Open()
    {
      device.Open();
      if (ModuleViewModels.Count > 0)
        SelectedModuleViewModel = ModuleViewModels[0];
    }

    public void Close()
    {
      device.Close();
    }
  }
}
