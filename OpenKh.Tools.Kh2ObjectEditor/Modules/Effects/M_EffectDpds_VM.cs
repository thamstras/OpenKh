using OpenKh.Tools.Kh2ObjectEditor.Services;
using static OpenKh.Tools.Kh2ObjectEditor.Modules.Model.ModuleModelMeshes_VM;
using System.Collections.ObjectModel;
using OpenKh.Kh2;
using static OpenKh.Tools.Kh2ObjectEditor.Modules.Effects.M_EffectDpdTexture_VM;
using System.Windows;

namespace OpenKh.Tools.Kh2ObjectEditor.Modules.Effects
{
    public class M_EffectDpds_VM
    {
        public ObservableCollection<DpdWrapper> DpdList { get; set; }

        public M_EffectDpds_VM()
        {
            DpdList = new ObservableCollection<DpdWrapper>();
            loadDpds();
        }

        public void loadDpds()
        {
            if (Apdx_Service.Instance.PaxFile?.DpxPackage?.DpdList == null || Apdx_Service.Instance.PaxFile.DpxPackage.DpdList.Count < 0)
                return;

            DpdList.Clear();
            for (int i = 0; i < Apdx_Service.Instance.PaxFile.DpxPackage.DpdList.Count; i++)
            {
                DpdWrapper wrapper = new DpdWrapper();
                wrapper.Id = i;
                wrapper.Name = "DPD " + i;
                wrapper.DpdItem = Apdx_Service.Instance.PaxFile.DpxPackage.DpdList[i];

                DpdList.Add(wrapper);
            }
        }
        public void Dpd_Copy(int index)
        {
            S_Clipboard.Instance.copyDpd(Apdx_Service.Instance.PaxFile.DpxPackage.DpdList[index]);
        }
        public void Dpd_AddCopied()
        {
            if (S_Clipboard.Instance.pasteDpd() == null)
                return;

            Apdx_Service.Instance.PaxFile.DpxPackage.DpdList.Add(S_Clipboard.Instance.pasteDpd());
            loadDpds();
        }
        public void Dpd_Replace(int index)
        {
            if (S_Clipboard.Instance.pasteDpd() == null)
                return;

            Apdx_Service.Instance.PaxFile.DpxPackage.DpdList[index] = S_Clipboard.Instance.pasteDpd();
            loadDpds();
        }

        public class DpdWrapper
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public Dpd DpdItem { get; set; }
        }
    }
}
