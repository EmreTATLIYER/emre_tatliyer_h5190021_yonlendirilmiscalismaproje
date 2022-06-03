using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace emre_tatliyer_h5190021_yonlendirilmiscalismaproje.Models
{
    public class SifremiYenilemeModel
    {    
        [Required(ErrorMessage = "Yeni şifre giriniz.", AllowEmptyStrings = false)]/*Hatalı/Boş girildiğinde çalışan mesaj komutu*/
        [DataType(DataType.Password)]/*Girilecek olan veritipini Şifre tipine çevirir*/
        public string YeniSifre { get; set; }

        [Required(ErrorMessage = "Doğrulama kodunu giriniz.", AllowEmptyStrings = false)]/*Hatalı/Boş girildiğinde çalışan mesaj komutu*/
        public string MusteriSifreKod { get; set; }
        
    }

}