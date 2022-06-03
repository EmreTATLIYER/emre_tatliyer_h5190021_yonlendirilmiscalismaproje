using emre_tatliyer_h5190021_yonlendirilmiscalismaproje.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.Security;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data;
using System.IO;



namespace emre_tatliyer_h5190021_yonlendirilmiscalismaproje.Controllers
{
    public class HomeController : Controller
    {
        public void rolGetir()
        {
            WebMarketDbEntities db = new WebMarketDbEntities();
            object id = Session["id"];
            var userInDb = db.TblMusteri.FirstOrDefault(x => x.MusteriId == (int)id);
            Session["rol"] = userInDb.MusteriRol;
            ViewBag.rol = Session["rol"];
        }

        [AllowAnonymous]
        public ActionResult Giris()
        {

            return View();
        }
        [AllowAnonymous]
        [HttpPost]
        public ActionResult Giris(TblMusteri user)
        {
            WebMarketDbEntities db = new WebMarketDbEntities();
            /* TblMusteride bulunan kolonla html kısmında bulunan inputları birbirine eşitler */
            var userInDb = db.TblMusteri.FirstOrDefault(a => a.MusteriEmail == user.MusteriEmail && a.MusteriSifre == user.MusteriSifre);

            /*Kullanıcının girdiği veriler tablo ile eşleşirse kullanıcıyı Anasayfa adlı safya yönlendirir*/

            if (userInDb != null)
            {
                FormsAuthentication.SetAuthCookie(userInDb.MusteriAd, false);
                Session["id"] = userInDb.MusteriId;
                Session["isim"] = userInDb.MusteriAd;
                Session["soyisim"] = userInDb.MusteriSoyad;
                Session["email"] = userInDb.MusteriEmail;
                Session["tel"] = userInDb.MusteriTel;
                Session["sifre"] = userInDb.MusteriSifre;
                Session["rol"] = userInDb.MusteriRol;


                return RedirectToAction("Anasayfa", "Home");

            }
            else
            {   /* Kullanıcının girdiği veriler yanlış ise mesaj komutu çalışır*/
                ViewBag.Mesaj = "E-Mail ya da Şifre Hatalı.";
                return View();
            }


        }


        [Authorize]
        public ActionResult Anasayfa()
        {
            object id = Session["id"];/*Giriş yapan kullanıcı Idisini Session ile taşır*/
            rolGetir();
            WebMarketDbEntities db = new WebMarketDbEntities();/*Database tanımlar */
            var deneme = db.TblUrunler.ToList();/* Databaseden gelen verileri listeler*/
            return View(deneme);

        }

        [AllowAnonymous]
        public ActionResult KayitOl()
        {

            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public ActionResult KayitOl(FormCollection form)
        {

            WebMarketDbEntities db = new WebMarketDbEntities();
            TblMusteri kayit = new TblMusteri();

            if (form != null)
            {
                kayit.MusteriAd = form["MusteriAd"].Trim();/*Girilen verileri tabloda bulunan kolona eşitler*/
                kayit.MusteriSoyad = form["MusteriSoyad"].Trim();/*Girilen verileri tabloda bulunan kolona eşitler*/
                kayit.MusteriEmail = form["MusteriEmail"].Trim();/*Girilen verileri tabloda bulunan kolona eşitler*/
                kayit.MusteriTel = form["MusteriTel"].Trim();/*Girilen verileri tabloda bulunan kolona eşitler*/
                kayit.MusteriSifre = form["MusteriSifre"].Trim();/*Girilen verileri tabloda bulunan kolona eşitler*/
                db.TblMusteri.Add(kayit);/*Tabloya verilen değerleri ekler*/
                db.SaveChanges();/*tabloyu Kayıt eder*/
                ViewBag.Kayit = "Hesap Oluşturuldu.";/* Başarılı bir kayıt olduğunda çalışan mesaj komutu*/
                return View();
            }
            else
            {
                ViewBag.Mesaj = "E-Mail zaten kayıtlı.";/*Hatalı kayıt olursa çalışan mesaj komutu*/
                return View("Giris");
            }
        }

        [AllowAnonymous]
        public ActionResult SifremiUnuttum()
        {
            return View();

        }


        [AllowAnonymous]
        [HttpPost]
        public ActionResult SifremiUnuttum(string Email)
        {
            using (WebMarketDbEntities db = new WebMarketDbEntities())
            {
                var account = db.TblMusteri.Where(a => a.MusteriEmail == Email).FirstOrDefault();
                if (account != null)
                {
                    Random rastgele = new Random();/* rastgele kod oluşturuyor*/
                    string harfler = "ABCDEFGHIJKLMNOPRSTUVYZabcdefghijklmnoprstuvyz0123456789";/*Rastgele oluşturmasını istediğim harfler*/
                    string resetCode = "";
                    for (int i = 0; i < 6; i++)/*Rastgele 6 tane harf ve rakam oluşturana kadar çalışan komut*/
                    {
                        resetCode += harfler[rastgele.Next(harfler.Length)];/*Oluşturulan rastgele harfler resetCodea eşitleniyor */
                    }
                    SendVerificationLinkEmail(account.MusteriEmail, resetCode, "SifremiYenileme");
                    account.MusteriSifreKodu = resetCode;/*Oluşturulan resetCode Tabloda bulunan MusteriSifreKodu kolona eşitlenir*/
                    db.Configuration.ValidateOnSaveEnabled = false;
                    db.SaveChanges();/*Database değişikleri kayıt eder*/
                    ViewBag.Message = "Şifre sıfırlama linki gönderildi.";/*Tabloda kayıtlı olan mail girdiğinde çalışan mesaj komutu*/
                    return RedirectToAction("Giris");/*Girilen Mail doğruysa kullanıcıyı Anasayfaya yönlendiriyor*/
                }
                else
                {   /*Kullanıcı tabloda kayıtlı olmayan mail girdiğinde çalışan mesaj komutu*/
                    ViewBag.Message = "Hesap bulunamadı.";
                }
            }

            return View();
        }
        [NonAction]
        
        public void SendVerificationLinkEmail(string email, string activationCode, string emailFor = "SifremiYenileme")
        {

            var verifyUrl = "/Home/" + emailFor + "/" + activationCode;
            var link = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, verifyUrl);

            var fromEmail = new MailAddress("Vicboss162@gmail.com");
            var toEmail = new MailAddress(email);
            var fromEmailPassword = "Joker3275";

            string subject = "Şifre Sıfırlama";
            string body = "Merhaba,<br>Şifrenizi altta bulunan linkten sıfırlayabilirisiniz.<br>Doğrulama kodunuz:" + activationCode +
                    "<br/><a href=" + link + ">Şifre sıfırlama</a>";


            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromEmail.Address, fromEmailPassword)

            };

            using (var message = new MailMessage(fromEmail, toEmail)
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            }
            )
                smtp.Send(message);
        }

        [AllowAnonymous]
        public ActionResult SifremiYenileme(string id)
        {

            if (string.IsNullOrWhiteSpace(id))
            {
                return HttpNotFound();/* Kullanıcıyı hata sayfasına yönlerdirir*/
            }

            using (WebMarketDbEntities dc = new WebMarketDbEntities())/*Veritabanını tanımlar*/
            {       /*Tabloda bulunan kolon ile id adlı değişkene eşitler*/
                var user = dc.TblMusteri.Where(a => a.MusteriSifreKodu == id).FirstOrDefault();
                if (user != null)
                {
                    SifremiYenilemeModel model = new SifremiYenilemeModel();/*modeli tanımlar*/
                    model.MusteriSifreKod = id;/* modelde bulunan MusteriSifreKod ile idyi eşitler*/
                    return View();
                }
                else
                {
                    return HttpNotFound();/* Kullanıcıyı hata sayfasına yönlerdirir*/
                }
            }
        }
        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SifremiYenileme(SifremiYenilemeModel login)
        {
            using (WebMarketDbEntities db = new WebMarketDbEntities())/*Veritabanını tanımlar*/
            {
                var detail = db.TblMusteri.Where(log => log.MusteriSifreKodu == login.MusteriSifreKod).FirstOrDefault();
                if (detail != null)
                {   /*Tabloda bulunan kolon ile Modellerde bulunan SifremiYenilemeModel eşitler*/
                    var userDetail = db.TblMusteri.FirstOrDefault(c => c.MusteriSifreKodu == login.MusteriSifreKod);


                    if (userDetail != null)
                    {
                        userDetail.MusteriSifre = login.YeniSifre;/*Girilen yeni şifreyi kayıtlı şifreye eşitler*/

                        db.SaveChanges();/*Yeni Şifreyi Dbye kayıt eder*/
                        ViewBag.Message = "Şifre Değiştirildi!";/*Şifre değiştirildiğinde çalışan mesaj komutu */
                    }
                    else
                    {
                        ViewBag.Message = "Şifre Değiştirilemedi!";/*Şifre değiştirilmediğinde çalışan mesaj komutu*/
                        return View();
                    }

                }
            }

            return View(login);
        }



        [HttpPost]
        [Authorize]
        public ActionResult Logout()
        {

            FormsAuthentication.SignOut();
            Session.Abandon();
            return RedirectToAction("Giris");
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult Ekle()
        {

            return View();
        }/*
        WebMarketDbEntities db = new WebMarketDbEntities();
        [HttpPost]
        public ActionResult Ekle(Tbl_Kategori k)

        {
            if (Request.Files.Count > 0)
            {
                string dosyaadi = Path.GetFileName(Request.Files[0].FileName);
                string uzanti = Path.GetExtension(Request.Files[0].FileName);
                string yol = "~/Image/" + dosyaadi + uzanti;
                Request.Files[0].SaveAs(Server.MapPath(yol));
                k.MagazaResmi = "/Image/" + dosyaadi + uzanti;
            }

            db.Tbl_Kategori.Add(k);
            db.SaveChanges();
            return View();
        } */
        [Authorize]
        [HttpPost]
        public ActionResult MagazaKayit(FormCollection form, int magazaturuid)
        {
            WebMarketDbEntities db = new WebMarketDbEntities();
            TblMagaza magazakayit = new TblMagaza();/*Değişkeni tanımlar*/
            object id = Session["id"];
            ViewBag.id = id;

            if (form != null)
            {
                var userInDb = db.TblMusteri.Where(x => x.MusteriId == (int)id).FirstOrDefault();
                userInDb.MusteriRol = "P";
                magazakayit.MusteriId = ViewBag.id;/*kullanıcının Idsini kayıt eder*/
                magazakayit.MagazaAdi = form["MagazaAdi"].Trim();/*Girilen verileri tabloda bulunan kolona eşitler*/
                magazakayit.MagazaTel = form["MagazaTel"].Trim();/*Girilen verileri tabloda bulunan kolona eşitler*/
                magazakayit.MagazaAdres = form["MagazaAdres"].Trim();/*Girilen verileri tabloda bulunan kolona eşitler*/
                magazakayit.MagazaTuruId = magazaturuid;/*kayıt ekranından seçilen veriyi ekler*/
                magazakayit.MagazaResmi = "/Image/Stock.png";/*resimi ekler*/

                db.TblMagaza.Add(magazakayit);/*Tabloya verilen değerleri ekler*/
                db.SaveChanges();/*tabloyu Kayıt eder*/
                ViewBag.Kayit = "Mağaza Oluşturuldu.";/* Başarılı bir kayıt olduğunda çalışan mesaj komutu*/
                return View();
            }

            return View();
        }
        [HttpGet]
        public ActionResult MagazaKayit()
        {
            ViewBag.id = Session["id"];
            ViewBag.isim = Session["isim"];
            ViewBag.soyisim = Session["soyisim"];
            ViewBag.tel = Session["tel"];
            ViewBag.email = Session["email"];

            return View();
        }

        [Authorize]

        [HttpPost]
        public ActionResult MusteriBilgileri(TblMusteri musteri)
        {
            object id = Session["id"];/*Kullanıcı giriş yaptıktan sonra bilgileri session ile taşınır*/
            object isim = Session["isim"];/*...*/
            object soyisim = Session["soyisim"];/*...*/
            object tel = Session["tel"];/*...*/
            object email = Session["email"];/*...*/
            object sifre = Session["sifre"];/*...*/
            ViewBag.id = id;/*alınan bilgiler viewbag yardımıyla html kısmına gönderip ekrana bastırılır*/
            ViewBag.isim = isim;/*...*/
            ViewBag.soyisim = soyisim;/*...*/
            ViewBag.tel = tel;/*...*/
            ViewBag.email = email;/*...*/
            ViewBag.sifre = sifre;/*...*/
            WebMarketDbEntities db = new WebMarketDbEntities();/* veritabanı tanımlanır*/
            {
                var userD = db.TblMusteri.Where(c => c.MusteriId == (int)id).FirstOrDefault();/*Kullanıcının Idsi alınır*/
                if (userD != null)
                {
                    userD.MusteriAd = musteri.MusteriAd;/*Veritabanı ile html kısmında girilen inputlar eşitlenir*/
                    userD.MusteriSoyad = musteri.MusteriSoyad;
                    userD.MusteriEmail = musteri.MusteriEmail;
                    userD.MusteriTel = musteri.MusteriTel;
                    userD.MusteriSifre = musteri.MusteriSifre;
                    db.SaveChanges();/* değişimler kayıt edilir*/
                    ViewBag.Bildirim = "Bilgi Değişimi başarılı!";
                }
                else
                {
                    ViewBag.Bildirim = "Bilgi Değişimi başarısız!";
                }
            }
            return View();
        }


        [HttpGet]
        public ActionResult MusteriBilgileri()
        {

            object id = Session["id"];/*Kullanıcı giriş yaptıktan sonra bilgileri session ile taşınır*/
            object isim = Session["isim"];/*...*/
            object soyisim = Session["soyisim"];/*...*/
            object tel = Session["tel"];/*...*/
            object email = Session["email"];/*...*/
            object sifre = Session["sifre"];/*...*/
            ViewBag.id = id;/*alınan bilgiler viewbag yardımıyla html kısmına gönderip ekrana bastırılır*/
            ViewBag.isim = isim;/*...*/
            ViewBag.soyisim = soyisim;/*...*/
            ViewBag.tel = tel;/*...*/
            ViewBag.email = email;/*...*/
            ViewBag.sifre = sifre;/*...*/


            return View();
        }

        public ActionResult Magazalarim()
        { object id = Session["id"];
            WebMarketDbEntities db = new WebMarketDbEntities();
            var model = db.TblMagaza.Where(e => e.MusteriId == (int)id).ToList();/*model adlı değişkeni tablo ile eşitleyip listeler*/
            return View(model);
        }
        [HttpPost]
        public ActionResult MagazaBilgileri(TblMagaza magaza, int id)
        {
            WebMarketDbEntities db = new WebMarketDbEntities();
            var magazadb = db.TblMagaza.Where(c => c.MagazaId == (int)id).FirstOrDefault();/*Mağaza Id alınıp bilgileri listelenir*/
            if (magazadb != null)
            {
                magazadb.MagazaAdi = magaza.MagazaAdi;/*Veritabanı ile html kısmında girilen inputlar eşitlenir*/
                magazadb.MagazaTel = magaza.MagazaTel;/*...*/
                magazadb.MagazaAdres = magaza.MagazaAdres;/*...*/
                magazadb.MagazaSure = magaza.MagazaSure;/*...*/
                magazadb.MagazaMinTutar = magaza.MagazaMinTutar;/*...*/
                magazadb.MagazaPuan = "*";
                db.SaveChanges();/*Dbyi Kayıt eder*/
                ViewBag.Bildirim = "Bilgi Değişimi başarılı!";
            }
            else
            {
                ViewBag.Bildirim = "Bilgi Değişimi başarısız!";
            }
            var model = db.TblMagaza.Where(e => e.MagazaId == id).FirstOrDefault();/*Mağaza Id alınıp bilgileri listelenir*/
            return View(model);
        }

        [HttpGet]
        public ActionResult MagazaBilgileri(int id)
        {
            WebMarketDbEntities db = new WebMarketDbEntities();
            var model = db.TblMagaza.Where(e => e.MagazaId == id).FirstOrDefault();/*Mağaza Id alınıp bilgileri listelenir*/
            return View(model);
        }




        [Authorize]
        public ActionResult Market()
        {
            rolGetir();
            /*Veritabanını tanımlar*/
            WebMarketDbEntities db = new WebMarketDbEntities();
            var model = db.TblMagaza.Where(e => e.MagazaTuruId == 10).ToList();/*model adlı değişkeni tablo ile eşitleyip listeler*/
            return View(model);

        }


        public ActionResult MarketMgz(int id)
        {
            rolGetir();
            WebMarketDbEntities db = new WebMarketDbEntities();/*Veritabanını tanımlar*/
            ViewModel magazaurun = new ViewModel();/*Modeli tanımlar*/
            magazaurun.magaza = db.TblMagaza.Where(e => e.MagazaId == id).ToList();/*Mağaza Idisini alıp listeler*/
            magazaurun.urunler = db.TblUrunler.Where(c => c.MagazaId == id).ToList();/*Mağaza Idisini alıp listeler*/
            var urunkategori = db.TblUrunler.ToList();/*Mağaza idleri alındıktan sonra ürünleri listeler */
            

            return View(magazaurun);

        }

        public ActionResult Manav()
        {
            rolGetir();
            WebMarketDbEntities db = new WebMarketDbEntities();/*Veritabanını tanımlar*/
            var model = db.TblMagaza.Where(e => e.MagazaTuruId == 11).ToList();/*model adlı değişkeni tablo ile eşitleyip listeler*/
            return View(model);

        }
    
        public ActionResult ManavMgz(int id)
        {
            rolGetir();
            WebMarketDbEntities db = new WebMarketDbEntities();/*Veritabanını tanımlar*/
            ViewModel magazaurun = new ViewModel();
            magazaurun.magaza = db.TblMagaza.Where(e => e.MagazaId == id).ToList();
            magazaurun.urunler = db.TblUrunler.Where(c => c.MagazaId == id).ToList();

           
            return View(magazaurun);
        }
        public ActionResult Kasap()
        {
            rolGetir();
            WebMarketDbEntities db = new WebMarketDbEntities();/*Veritabanını tanımlar*/
            var model = db.TblMagaza.Where(e => e.MagazaTuruId == 12).ToList();/*model adlı değişkeni tablo ile eşitleyip listeler*/
            return View(model);

        }
        public ActionResult KasapMgz(int id)
        {
            WebMarketDbEntities db = new WebMarketDbEntities();/*Veritabanını tanımlar*/
            var model = db.TblMagaza.Where(e => e.MagazaId == id).ToList();/*model adlı değişkeni tablo ile eşitleyip listeler*/


            return View(model);
        }
        public ActionResult Kuruyemisci()
        {
            rolGetir();
            WebMarketDbEntities db = new WebMarketDbEntities();/*Veritabanını tanımlar*/
            var model = db.TblMagaza.Where(e => e.MagazaTuruId == 13).ToList();/*model adlı değişkeni tablo ile eşitleyip listeler*/
            return View(model);

        }
        public ActionResult KuruyemisciMgz(int id)
        {
            WebMarketDbEntities db = new WebMarketDbEntities();/*Veritabanını tanımlar*/
            var model = db.TblMagaza.Where(e => e.MagazaId == id).ToList();/*model adlı değişkeni tablo ile eşitleyip listeler*/


            return View(model);
        }
        public ActionResult Petshop()
        {
            rolGetir();
            WebMarketDbEntities db = new WebMarketDbEntities();/*Veritabanını tanımlar*/
            var model = db.TblMagaza.Where(e => e.MagazaTuruId == 14).ToList();/*model adlı değişkeni tablo ile eşitleyip listeler*/
            return View(model);

        }
        public ActionResult PetshopMgz(int id)
        {
            WebMarketDbEntities db = new WebMarketDbEntities();/*Veritabanını tanımlar*/
            var model = db.TblMagaza.Where(e => e.MagazaId == id).ToList();/*model adlı değişkeni tablo ile eşitleyip listeler*/


            return View(model);
        }

        [HttpGet]
        public ActionResult MusteriAdres() 
        {
            object id = Session["id"];
            WebMarketDbEntities db = new WebMarketDbEntities();
            var model = db.TblMusteriAdres.Where(e => e.MusteriId == (int)id).ToList();/*model adlı değişkeni tablo ile eşitleyip listeler*/
            return View(model); 
        }
        [HttpPost]
        public ActionResult MusteriAdres(FormCollection form)
        {
            WebMarketDbEntities db = new WebMarketDbEntities();
            TblMusteriAdres adreskayit = new TblMusteriAdres();/*değişken tanımlar*/
            object id = Session["id"];/*kullanıcı Idsi taşır*/
            ViewBag.id = id;
            var aktifadresdb = db.TblMusteriAdres.Where(X => X.MusteriAdresDurum == "A").Select(x => x.MusteriAdresId);/*Dbden gelen aktif adresi seçer*/

            if (form != null)
            {


                adreskayit.MusteriId = ViewBag.id;/*Yeni kayıt için kullanıcı Idsini ekler*/
                adreskayit.MusteriAdresAdi = form["MusteriAdresAdi"].Trim();/*Girilen verileri tabloda bulunan kolona eşitler*/
                adreskayit.MusteriAdresIl = form["MusteriAdresIl"].Trim();/*Girilen verileri tabloda bulunan kolona eşitler*/
                adreskayit.MusteriAdresIlce = form["MusteriAdresIlce"].Trim();/*Girilen verileri tabloda bulunan kolona eşitler*/
                adreskayit.MusteriAdres = form["MusteriAdres"].Trim();/*Dbden gelen aktif kartı seçer*/
                adreskayit.MusteriAdresDurum = "P";/*Adresi Pasif olarak kayıt eder*/


                db.TblMusteriAdres.Add(adreskayit);/*Tabloya verilen değerleri ekler*/
                db.SaveChanges();/*tabloyu Kayıt eder*/
                ViewBag.Kayitadres = "Adres Kaydedildi.";/* Başarılı bir kayıt olduğunda çalışan mesaj komutu*/
                return RedirectToAction("MusteriAdres", "Home");
            }
            return View();
        }

        [HttpGet]
        public ActionResult MusteriAdresDuzenle(int id)
        {
            WebMarketDbEntities db = new WebMarketDbEntities();
            TblMusteriAdres adresgoruntule = new TblMusteriAdres();/*değişkeni tanımlar*/
            var model = db.TblMusteriAdres.Where(e => e.MusteriAdresId == id).FirstOrDefault();/*Adres Idsini alır*/
            return View(model);
        }
        [HttpPost]
        public ActionResult MusteriAdresDuzenle(TblMusteriAdres adres, int musteriadresid) 
        {
            WebMarketDbEntities db = new WebMarketDbEntities();
            {
                var adresdb = db.TblMusteriAdres.Where(c => c.MusteriAdresId == musteriadresid).FirstOrDefault();/*Adres Idsini alır*/
                if (adresdb != null)
                {
                    adresdb.MusteriAdresId = musteriadresid;/*alınan Idyi kayıt eder*/
                    adresdb.MusteriAdresAdi = adres.MusteriAdresAdi;/*Değiştirilen yeri kayıt eder*/
                    adresdb.MusteriAdresIl = adres.MusteriAdresIl;/*Değiştirilen yeri kayıt eder*/
                    adresdb.MusteriAdresIlce = adres.MusteriAdresIlce;/*Değiştirilen yeri kayıt eder*/
                    adresdb.MusteriAdres = adres.MusteriAdres;/*Değiştirilen yeri kayıt eder*/

                    db.SaveChanges();/*dbye kayıt eder*/
                    ViewBag.Bildirim = "Bilgi Değişimi başarılı!";
                }
                else
                {
                    ViewBag.Bildirim = "Bilgi Değişimi başarısız!";
                }

            }
            return RedirectToAction("MusteriAdres", "Home"); 
        }
        public ActionResult MusteriAdresSil(int id)
        {
            WebMarketDbEntities db = new WebMarketDbEntities();
            var adresdb = db.TblMusteriAdres.Where(X=> X.MusteriAdresId == id ).FirstOrDefault();/*Adresin Idsini seçer*/
            db.TblMusteriAdres.Remove(adresdb);/*Dbden adresi siler*/
            db.SaveChanges();/*Dbyi kayıt eder*/
            return RedirectToAction("MusteriAdres", "Home");
        }

        public ActionResult MusteriAdresAktif(int id)
        {
            WebMarketDbEntities db = new WebMarketDbEntities();
            var pasifadresdb = db.TblMusteriAdres.Where(X => X.MusteriAdresId == id ) .FirstOrDefault();/*Adresin Idsini alır*/
            var aktifadresdb = db.TblMusteriAdres.Where(X => X.MusteriAdresDurum == "A" ).FirstOrDefault();/*Dbden Durumu "A"ya eşit olanı alır*/
            pasifadresdb.MusteriAdresDurum = "A";
                if (aktifadresdb != null)
                {
                aktifadresdb.MusteriAdresDurum = "P";/*Durumu aktif ise Pasife çevirir*/
                }           
            db.SaveChanges();/*Dbyi Kayıt eder*/
            return RedirectToAction("MusteriAdres","Home");
        }
        [HttpGet]
        public ActionResult MusteriKart()
        {
            object id = Session["id"];/*kullanıcı idsini taşır*/
            WebMarketDbEntities db = new WebMarketDbEntities();
            var model = db.TblMusteriKart.Where(e => e.MusteriId == (int)id).ToList();/*model adlı değişkeni tablo ile eşitleyip listeler*/
            return View(model);            
        }

        [HttpPost]
        public ActionResult MusteriKart(FormCollection form)
        {
            WebMarketDbEntities db = new WebMarketDbEntities();
            TblMusteriKart kartkayit = new TblMusteriKart();/*Model tanımlar*/
            object id = Session["id"];
            ViewBag.id = id;
            var aktifkartdb = db.TblMusteriKart.Where(X => X.MusteriKartDurum == "A").Select(x => x.MusteriKartId);/*Dbden gelen aktif kartı seçer*/

            if (form != null)
            {
                kartkayit.MusteriId = ViewBag.id;/*kullanıcı Idsini Db kayıtı için ekler*/
                kartkayit.MusteriKartAdi = form["MusteriKartAdi"].Trim();/*Girilen verileri tabloda bulunan kolona eşitler*/
                kartkayit.MusteriKartNo = form["MusteriKartNo"].Trim();/*Girilen verileri tabloda bulunan kolona eşitler*/
                kartkayit.MusteriKartUstuIsim = form["MusteriKartUstuIsim"].Trim();/*Girilen verileri tabloda bulunan kolona eşitler*/
                kartkayit.MusteriKartTarih = form["MusteriKartTarih"].Trim();/*Girilen verileri tabloda bulunan kolona eşitler*/
                kartkayit.MusteriKartCV = form["MusteriKartCV"].Trim();/*Girilen verileri tabloda bulunan kolona eşitler*/
                kartkayit.MusteriKartDurum = "P";/*Kayıt edilen kartı Pasif olarak kayıt eder*/

                db.TblMusteriKart.Add(kartkayit);/*Tabloya verilen değerleri ekler*/
                db.SaveChanges();/*tabloyu Kayıt eder*/
                ViewBag.Kayitadres = "Kart Kaydedildi.";/* Başarılı bir kayıt olduğunda çalışan mesaj komutu*/
                return RedirectToAction("MusteriKart", "Home");
            }
            else
            {
            }

            return View();
        }
        [HttpGet]
        public ActionResult MusteriKartDuzenle(int id)
        {
            WebMarketDbEntities db = new WebMarketDbEntities();
            TblMusteriKart kartgoruntule = new TblMusteriKart();
            var model = db.TblMusteriKart.Where(e => e.MusteriKartId == id).FirstOrDefault();
            return View(model);
        }
        [HttpPost]
        public ActionResult MusteriKartDuzenle(TblMusteriKart kart, int musterikartid)
        {
            WebMarketDbEntities db = new WebMarketDbEntities();

            {
                var kartdb = db.TblMusteriKart.Where(c => c.MusteriKartId == musterikartid).FirstOrDefault();
                if (kartdb != null)
                {
                    kartdb.MusteriKartId = musterikartid;
                    kartdb.MusteriKartAdi = kart.MusteriKartAdi;
                    kartdb.MusteriKartNo = kart.MusteriKartNo;
                    kartdb.MusteriKartUstuIsim = kart.MusteriKartUstuIsim;
                    kartdb.MusteriKartTarih = kart.MusteriKartTarih;
                    kartdb.MusteriKartCV = kart.MusteriKartCV;


                    db.SaveChanges();
                    ViewBag.Bildirim = "Bilgi Değişimi başarılı!";
                }
                else
                {
                    ViewBag.Bildirim = "Bilgi Değişimi başarısız!";
                }

            }
            return RedirectToAction("MusteriKart", "Home");
        }
        public ActionResult MusteriKartSil(int id)
        {
            WebMarketDbEntities db = new WebMarketDbEntities();
            var kartdb = db.TblMusteriKart.Where(X => X.MusteriKartId == id).FirstOrDefault();/*Dbden gelen kart Idyi alır*/

            db.TblMusteriKart.Remove(kartdb);/*Dbden silme işlemi yapar*/

            db.SaveChanges();/*Dbyi kayıt eder*/

            return RedirectToAction("MusteriKart", "Home");
        }

        public ActionResult MusteriKartAktif(int id)
        {
            WebMarketDbEntities db = new WebMarketDbEntities();
            var pasifkartdb = db.TblMusteriKart.Where(X => X.MusteriKartId == id).FirstOrDefault();/*Dbden kart Idsini alır*/
            var aktifkartdb = db.TblMusteriKart.Where(X => X.MusteriKartDurum == "A").FirstOrDefault();/*Dbden Durumu "A"ya eşit olanı alır*/

            pasifkartdb.MusteriKartDurum = "A";
            if (aktifkartdb != null)
            {
                aktifkartdb.MusteriKartDurum = "P";/*Eğer kart aktifse Pasife çevirir*/
            }

            db.SaveChanges();/*Dbyi Kayıt eder*/

            return RedirectToAction("MusteriKart", "Home");
        }       
        
        [HttpGet]
        public ActionResult MagazaUrunEkle( int id) 
        {
            WebMarketDbEntities db = new WebMarketDbEntities();
            TblMagaza magazaturu = new TblMagaza();/*Değişken tanımlar*/
            var magazakategori = db.TblMagaza.Where(b => b.MagazaId == id).FirstOrDefault();/*mağaza Idyi seçer*/
            ViewBag.magazakategori = magazakategori.MagazaTuruId;        
            return View(); 
        }
     
        [HttpPost]
        public ActionResult MagazaUrunEkle(FormCollection form, int urunkategori, int id) 
        {
            WebMarketDbEntities db = new WebMarketDbEntities();
            TblUrunler urunkayit = new TblUrunler();/*Değişken tanımlar*/
            TblMagaza magazaturu = new TblMagaza();/*Değişken tanımlar*/
            var magazakategori = db.TblMagaza.Where(b=> b.MagazaId == id).Select(x => x.MagazaTuruId);/*seçilen Idnin Mağaza türünü alır*/
                                
            if (form != null)
            {
                urunkayit.MagazaId = id;/*Idyi ekler*/
                urunkayit.UrunAdi = form["UrunAdi"].Trim();/*Girilen verileri tabloda bulunan kolona eşitler*/
                urunkayit.UrunFiyat = form["UrunFiyat"].Trim();/*Girilen verileri tabloda bulunan kolona eşitler*/
                urunkayit.UrunStock = form["UrunStock"].Trim();/*Girilen verileri tabloda bulunan kolona eşitler*/
                urunkayit.UrunAciklama = form["UrunAciklama"].Trim();/*Girilen verileri tabloda bulunan kolona eşitler*/
                urunkayit.UrunKategorisi = urunkategori;/*Html kısmında bulunan Dropboxtaki değeri ekler*/
                urunkayit.UrunTuru = form["urunturu"].Trim();/*Girilen verileri tabloda bulunan kolona eşitler*/

                if (Request.Files.Count > 0)
                {
                    string dosyaadi = Path.GetFileName(Request.Files[0].FileName);/*dosya adını ekler*/
                    string uzanti = Path.GetExtension(Request.Files[0].FileName);/*dosya uzantısını ekler örn:png,jpg*/
                    string yol = "~/ImageUrun/" + dosyaadi + uzanti;/* eklenen dosyanın yolunu belirtir*/
                    Request.Files[0].SaveAs(Server.MapPath(yol));/*dosyayı kayıt eder*/
                    urunkayit.UrunResim = "/ImageUrun/" + dosyaadi + uzanti;
                }
                db.TblUrunler.Add(urunkayit);/*Tabloya verilen değerleri ekler*/
                db.SaveChanges();/*tabloyu Kayıt eder*/
                ViewBag.Kayit = "Ürün Oluşturuldu.";/* Başarılı bir kayıt olduğunda çalışan mesaj komutu*/
                return View();
            }
            return View();
        }
        
        public ActionResult MagazaUrun(int? id)
        {
            WebMarketDbEntities db = new WebMarketDbEntities();/*Veritabanını tanımlar*/
            ViewModel magazaurun = new ViewModel();/*Değişkeni tanımlar*/
            magazaurun.magaza = db.TblMagaza.Where(e => e.MagazaId == id).ToList();/*Mağaza Idsi ile bilgileri listelerr*/
            magazaurun.urunler = db.TblUrunler.Where(c => c.MagazaId == id).ToList();/*mağaza Idsine ait ürünleri listeler*/
            
            return View(magazaurun);
        }
        public void magazaTurGetir(int id)
        {
            WebMarketDbEntities db = new WebMarketDbEntities();
            ViewModel magazaid = new ViewModel();
            magazaid.magazaid = db.TblMagaza.Where(a => a.MagazaId == id).FirstOrDefault();


            View(magazaid);
        }
        [HttpGet]
        public ActionResult MagazaUrunDuzenle(int id, int magazaturuid, int urunid)
        {
            WebMarketDbEntities db = new WebMarketDbEntities();
            ViewModel urungoruntule = new ViewModel();/*değişkeni tanımlar*/
            urungoruntule.magaza = db.TblMagaza.Where(e => e.MagazaId == id).ToList();/*Dbden mağazanın Idsini alır*/
            urungoruntule.urunler = db.TblUrunler.Where(a => a.UrunId == urunid).ToList();/*Dbden Ürünün Idsini alır*/
            
            return View(urungoruntule);
        }
        [HttpPost]
        public ActionResult MagazaUrunDuzenle(TblUrunler urun, int urunid,int id)
        {
            WebMarketDbEntities db = new WebMarketDbEntities();
            ViewModel urungoruntule = new ViewModel();/*değişkeni tanımlar*/                   
            {
                var urundb = db.TblUrunler.Where(c => c.UrunId == urunid).FirstOrDefault();/*alınan Idyi seçer*/
                if (urundb != null)
                {
                    urundb.UrunId = urunid;/*Alının Idyi kayıt eder*/
                    urundb.UrunAdi = urun.UrunAdi;/*değiştirilen veriyi eşitler*/
                    urundb.UrunFiyat = urun.UrunFiyat;/*değiştirilen veriyi eşitler*/
                    urundb.UrunStock = urun.UrunStock;/*değiştirilen veriyi eşitler*/
                    urundb.UrunAciklama = urun.UrunAciklama;/*değiştirilen veriyi eşitler*/
                    urundb.UrunKategorisi = urun.UrunKategorisi;/*değiştirilen veriyi eşitler*/
                    urundb.UrunTuru = urun.UrunTuru;/*değiştirilen veriyi eşitler*/

                    db.SaveChanges();/*Dbye kayit eder*/
                    ViewBag.Bildirim = "Bilgi Değişimi başarılı!";                    
                }
                else
                {
                    ViewBag.Bildirim = "Bilgi Değişimi başarısız!";
                    return View();
                }
            }
            return RedirectToAction("MagazaUrun", "Home", new { id});

        }


    }
}
