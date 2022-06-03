using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace emre_tatliyer_h5190021_yonlendirilmiscalismaproje.Models
{
    public class ViewModel
    {
        public IEnumerable<TblMagaza>magaza{ get; set; }
        public TblMagaza magazaid { get; set; }
        public IEnumerable<TblUrunler>urunler{ get; set; }
        public TblUrunler urun { get; set; }

    }
}