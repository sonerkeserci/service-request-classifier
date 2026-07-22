using Microsoft.EntityFrameworkCore;
using RequestClassifier.Domain.Entities;
using RequestClassifier.Infrastructure.Data;

namespace RequestClassifier.Infrastructure.Data.Seed;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        if (await context.Departments.AnyAsync()) // Skip seeding if departments already exist.
            return;

        /*
        Departments and RequestCategories will be added here.
        This method will be called once in Program.cs, then disabled.
        */

        var departments = new List<Department>
        {
            CreateDepartment(
                "Fen İşleri Dairesi",
                "FEN",
                "Yol, kaldırım, altyapı, drenaj, sanat yapıları ve kışla mücadele hizmetlerini yürütür.",
                (
                        "Yol, Asfalt ve Kaldırım",
                        "YOL",
                        "Yol yapımı, asfaltlama, yol çukurları, kaldırım, bordür ve yol bakım taleplerini kapsar."
                    ),
                    (
                        "Kazı ve Altyapı",
                        "KAZI",
                        "Kazı çalışmaları, altyapı müdahaleleri ve kazı sonrasında oluşan bozulmaları kapsar."
                    ),
                    (
                        "Yağmur Suyu ve Drenaj",
                        "DRENAJ",
                        "Yağmur suyu kanalları, drenaj sistemleri, mazgallar ve su birikmesi sorunlarını kapsar."
                    ),
                    (
                        "Köprü, Menfez ve İstinat",
                        "KOPRU",
                        "Köprü, menfez ve istinat duvarlarının yapım, bakım ve onarım taleplerini kapsar."
                    ),
                    (
                        "Kar ve Buzlanma",
                        "KAR",
                        "Kar küreme, yol açma, tuzlama ve buzlanmayla mücadele taleplerini kapsar."
                    )
                ),

            CreateDepartment(
                "İmar ve Şehircilik Dairesi",
                "IMAR",
                "İmar planları, yapılaşma, ruhsat, adres, parsel ve kentsel dönüşüm işlemlerini yürütür.",
                (
                    "Kaçak Yapı",
                    "KACAK",
                    "Ruhsatsız, izinsiz veya imar mevzuatına aykırı yapılaşma bildirimlerini kapsar."
                ),
                (
                    "İmar Durumu",
                    "IMARDUR",
                    "Taşınmazların imar durumu, yapılaşma koşulları ve plan bilgilerine ilişkin talepleri kapsar."
                ),
                (
                    "Yapı Ruhsatı ve İskân",
                    "RUHSAT",
                    "Yapı ruhsatı, ruhsat yenileme ve yapı kullanma izin belgesi işlemlerini kapsar."
                ),
                (
                    "Numarataj ve Adres",
                    "ADRES",
                    "Bina numarası, kapı numarası, sokak adı ve adres kayıtlarına ilişkin talepleri kapsar."
                ),
                (
                    "Harita ve Parsel",
                    "PARSEL",
                    "Harita, parsel, ada, sınır ve taşınmaz konum bilgilerine ilişkin talepleri kapsar."
                ),
                (
                    "Kentsel Dönüşüm",
                    "KDONUSUM",
                    "Riskli alan, riskli yapı ve kentsel dönüşüm uygulamalarına ilişkin talepleri kapsar."
                )
            ),

            CreateDepartment(
                "Ulaşım ve Trafik Dairesi",
                "ULSM",
                "Toplu taşıma, trafik düzeni, duraklar, otoparklar ve ulaşım kartı hizmetlerini yürütür.",
                (
                    "Toplu Taşıma",
                    "TOPLUTAS",
                    "Otobüs seferleri, güzergâhlar, yolcu taşımacılığı ve toplu taşıma hizmetlerini kapsar."
                ),
                (
                    "Otobüs Durağı",
                    "DURAK",
                    "Durak yeri, durak kurulması, durak bakımı ve durak donanımlarına ilişkin talepleri kapsar."
                ),
                (
                    "Trafik Düzenleme ve Sinyalizasyon",
                    "TRAFIK",
                    "Trafik ışıkları, levhalar, yol çizgileri ve kavşak düzenlemelerine ilişkin talepleri kapsar."
                ),
                (
                    "Otopark",
                    "OTOPARK",
                    "Belediye otoparkları, park alanları ve otopark düzenine ilişkin talepleri kapsar."
                ),
                (
                    "Ulaşım Kartı",
                    "ULSKART",
                    "Ulaşım kartı başvurusu, bakiye, ücretlendirme ve kart arızalarına ilişkin talepleri kapsar."
                )
            ),

            CreateDepartment(
                "Çevre Koruma ve Sıfır Atık Dairesi",
                "CEVRE",
                "Atık yönetimi, geri dönüşüm, çevre kirliliği ve hafriyat denetimi hizmetlerini yürütür.",
                (
                    "Çöp ve Atık",
                    "COPATIK",
                    "Evsel atıklar, çöp birikmesi, konteyner ve atık toplama sorunlarını kapsar."
                ),
                (
                    "Geri Dönüşüm",
                    "GERIDON",
                    "Geri dönüşüm kutuları, ayrıştırma ve geri dönüştürülebilir atık taleplerini kapsar."
                ),
                (
                    "Çevre Kirliliği",
                    "CEVKIR",
                    "Hava, su, toprak, görüntü ve genel çevre kirliliği bildirimlerini kapsar."
                ),
                (
                    "Gürültü Kirliliği",
                    "GURULTU",
                    "İş yeri, etkinlik, inşaat ve çevresel kaynaklı aşırı gürültü şikâyetlerini kapsar."
                ),
                (
                    "Hafriyat Atığı",
                    "HAFRIYAT",
                    "İzinsiz hafriyat dökümü, inşaat atıkları ve moloz birikmesi bildirimlerini kapsar."
                )
            ),

            CreateDepartment(
                "Park, Bahçe ve Kent Estetiği Dairesi",
                "PARK",
                "Parklar, yeşil alanlar, oyun alanları, kent mobilyaları ve peyzaj hizmetlerini yürütür.",
                (
                    "Park ve Oyun Alanı",
                    "PARKOYUN",
                    "Parklar, çocuk oyun alanları, spor alanları ve bu alanlardaki bakım sorunlarını kapsar."
                ),
                (
                    "Ağaç ve Budama",
                    "AGAC",
                    "Ağaç dikimi, budama, tehlikeli dallar ve ağaçların bakımına ilişkin talepleri kapsar."
                ),
                (
                    "Yeşil Alan ve Sulama",
                    "YESILAL",
                    "Çim alanlar, bitkilendirme, sulama sistemleri ve yeşil alan bakım taleplerini kapsar."
                ),
                (
                    "Kent Mobilyası",
                    "KENTMOB",
                    "Bank, çöp kutusu, kamelya ve benzeri kent mobilyalarına ilişkin talepleri kapsar."
                ),
                (
                    "Peyzaj ve Kent Estetiği",
                    "PEYZAJ",
                    "Peyzaj düzenlemesi, kent görünümü ve estetik iyileştirme taleplerini kapsar."
                )
            ),

            CreateDepartment(
                "Kırsal ve Tarımsal Hizmetler Dairesi",
                "KIRSL",
                "Kırsal mahallelere, çiftçilere, üreticilere ve hayvancılığa yönelik hizmetleri yürütür.",
                (
                    "Tarımsal Destek",
                    "TARDEST",
                    "Tohum, fide, ekipman ve diğer tarımsal destek başvurularını kapsar."
                ),
                (
                    "Hayvancılık Desteği",
                    "HAYDEST",
                    "Yem, hayvan yetiştiriciliği ve hayvancılık projelerine yönelik destek taleplerini kapsar."
                ),
                (
                    "Tarımsal Sulama",
                    "TARSUL",
                    "Tarımsal sulama kanalları, su temini ve sulama altyapısı taleplerini kapsar."
                ),
                (
                    "Kırsal Yol ve Altyapı",
                    "KIRYOL",
                    "Kırsal mahallelerdeki yol, geçit ve temel altyapı sorunlarını kapsar."
                ),
                (
                    "Üretici Desteği",
                    "URETICI",
                    "Yerel üretici, kooperatif, pazarlama ve üretim geliştirme taleplerini kapsar."
                )
            ),

            CreateDepartment(
                "Veteriner İşleri Dairesi",
                "VET",
                "Sahipsiz hayvanların korunması, tedavisi, barınması ve güvenliğine yönelik hizmetleri yürütür.",
                (
                    "Sahipsiz Hayvan",
                    "SAHIPSIZ",
                    "Sahipsiz hayvanların toplanması, korunması ve bulunduğu bölgeye ilişkin bildirimleri kapsar."
                ),
                (
                    "Yaralı ve Hasta Hayvan",
                    "YARALI",
                    "Yaralı, hasta veya acil veteriner müdahalesine ihtiyaç duyan hayvan bildirimlerini kapsar."
                ),
                (
                    "Barınak ve Kısırlaştırma",
                    "BARINAK",
                    "Hayvan barınağı, rehabilitasyon ve kısırlaştırma hizmetlerine ilişkin talepleri kapsar."
                ),
                (
                    "Hayvan Kaynaklı Tehlike",
                    "HAYTEH",
                    "Saldırgan hayvan, sürü halinde dolaşma ve can güvenliği riski oluşturan durumları kapsar."
                )
            ),

            CreateDepartment(
                "Zabıta ve Ruhsat Dairesi",
                "ZAB",
                "Belediye düzeninin korunması, iş yeri ruhsatları ve ticari faaliyetlerin denetimini yürütür.",
                (
                    "Zabıta",
                    "ZABITA",
                    "Seyyar satıcı, kaldırım işgali, dilencilik ve kamu düzenine ilişkin şikâyetleri kapsar."
                ),
                (
                    "Ruhsat",
                    "ISYRUH",
                    "İş yeri açma ruhsatı, ruhsatsız faaliyet ve ruhsat işlemlerine ilişkin talepleri kapsar."
                ),
                (
                    "Denetim",
                    "DENETIM",
                    "Fiyat, etiket, pazar yeri, hijyen ve ticari işletme denetimi taleplerini kapsar."
                )
            ),

            CreateDepartment(
                "İtfaiye ve Afet İşleri Dairesi",
                "ITF",
                "Yangın, kurtarma, acil müdahale, afet hazırlığı ve risk azaltma hizmetlerini yürütür.",
                (
                    "Yangın",
                    "YANGIN",
                    "Yangın ihbarları ve yangın güvenliğine ilişkin acil bildirimleri kapsar."
                ),
                (
                    "Kurtarma ve Acil Müdahale",
                    "KURTARMA",
                    "Kaza, mahsur kalma ve benzeri durumlarda kurtarma ve acil müdahale taleplerini kapsar."
                ),
                (
                    "İtfaiye Uygunluk İşlemleri",
                    "ITFUYG",
                    "İş yeri ve yapılara yönelik itfaiye uygunluk raporu işlemlerini kapsar."
                ),
                (
                    "Afet ve Risk Bildirimi",
                    "AFET",
                    "Afet riski, tehlikeli yapı, toplanma alanı ve afet hazırlığına ilişkin bildirimleri kapsar."
                )
            ),

            CreateDepartment(
                "Sosyal Hizmetler Dairesi",
                "SOSYL",
                "İhtiyaç sahibi vatandaşlara yönelik sosyal yardım, bakım ve destek hizmetlerini yürütür.",
                (
                    "Sosyal Yardım",
                    "SOSYARD",
                    "Gıda, yakacak, maddi destek ve diğer genel sosyal yardım başvurularını kapsar."
                ),
                (
                    "Engelli ve Yaşlı Desteği",
                    "ENGYAS",
                    "Engelli ve yaşlı vatandaşlara yönelik bakım, ulaşım ve destek taleplerini kapsar."
                ),
                (
                    "Kadın ve Aile Desteği",
                    "KADAILE",
                    "Kadınlara, çocuklara ve ailelere yönelik danışmanlık ve destek taleplerini kapsar."
                ),
                (
                    "Barınma Desteği",
                    "BARDEST",
                    "Geçici barınma, konaklama ve barınma ihtiyacına yönelik yardım taleplerini kapsar."
                )
            ),

            CreateDepartment(
                "Kültür, Gençlik ve Spor Dairesi",
                "KGS",
                "Kültür, sanat, eğitim, gençlik ve spor faaliyetlerinin planlanmasını ve yürütülmesini sağlar.",
                (
                    "Etkinlikler",
                    "ETKINLIK",
                    "Kültür, sanat, festival, konser, sergi ve benzeri etkinlik taleplerini kapsar."
                ),
                (
                    "Kurs ve Eğitim",
                    "KURSEG",
                    "Belediye kursları, atölyeler ve eğitim programlarına ilişkin başvuruları kapsar."
                ),
                (
                    "Spor Hizmetleri",
                    "SPOR",
                    "Spor tesisleri, sportif etkinlikler ve spor faaliyetlerine ilişkin talepleri kapsar."
                ),
                (
                    "Gençlik Hizmetleri",
                    "GENCLIK",
                    "Gençlik merkezleri ve gençlere yönelik proje ve faaliyet taleplerini kapsar."
                )
            ),

            CreateDepartment(
                "Sağlık İşleri Dairesi",
                "SAG",
                "Vatandaşlara yönelik belediye sağlık, bakım, nakil ve ilaçlama hizmetlerini yürütür.",
                (
                    "Sağlık Hizmetleri",
                    "SAGHIZ",
                    "Belediyenin sunduğu genel sağlık hizmetleri ve sağlık destek taleplerini kapsar."
                ),
                (
                    "Hasta Nakli",
                    "HASTANAK",
                    "Hasta nakil aracı ve sağlık kuruluşuna ulaşım taleplerini kapsar."
                ),
                (
                    "Evde Bakım",
                    "EVDEBAK",
                    "Yaşlı, engelli veya bakıma muhtaç vatandaşlara yönelik evde bakım taleplerini kapsar."
                ),
                (
                    "İlaçlama",
                    "ILACLAMA",
                    "Sivrisinek, haşere ve zararlılarla mücadele için ilaçlama taleplerini kapsar."
                )
            ),

            CreateDepartment(
                "Mezarlıklar ve Cenaze Hizmetleri Dairesi",
                "MEZAR",
                "Defin, mezarlık bakımı, mezar yeri ve cenaze nakli hizmetlerini yürütür.",
                (
                    "Defin ve Mezarlık İşlemleri",
                    "DEFIN",
                    "Defin işlemleri, mezar yeri tahsisi ve mezarlık kayıtlarına ilişkin talepleri kapsar."
                ),
                (
                    "Mezarlık Bakım ve Temizliği",
                    "MEZBAK",
                    "Mezarlıkların temizliği, bakımı ve çevre düzenlemesine ilişkin talepleri kapsar."
                ),
                (
                    "Cenaze Nakli",
                    "CENAZE",
                    "Cenazenin belediye araçlarıyla taşınmasına ilişkin talepleri kapsar."
                )
            ),

            CreateDepartment(
                "Bilgi İşlem Dairesi",
                "BILGI",
                "Vatandaşların kullandığı belediye dijital hizmetleri ve halka açık ağ sistemlerini yönetir.",
                (
                    "Dijital Hizmet ve Uygulama Sorunları",
                    "DIJITAL",
                    "E-belediye, mobil uygulama, çevrim içi başvuru ve dijital hizmetlerdeki sorunları kapsar."
                ),
                (
                    "İnternet ve Ağ Hizmetleri",
                    "INTERNET",
                    "Belediyenin halka sunduğu kablosuz internet ve ağ hizmetlerine ilişkin sorunları kapsar."
                )
            ),

            CreateDepartment(
                "Basın Yayın ve Halkla İlişkiler Dairesi",
                "BYHI",
                "Belediye ile vatandaş arasındaki iletişimi, bilgilendirmeyi ve kurumsal tanıtımı yürütür.",
                (
                    "Bilgi Talebi",
                    "BILGITAL",
                    "Belediye hizmetleri ve faaliyetleri hakkında bilgi edinme taleplerini kapsar."
                ),
                (
                    "Öneri ve Görüş",
                    "ONERIGOR",
                    "Belediye hizmetlerine ilişkin vatandaş önerilerini, görüşlerini ve geri bildirimlerini kapsar."
                ),
                (
                    "Kurumsal İletişim",
                    "KURILET",
                    "Belediyeye ulaşma, iletişim kanalları ve kurumsal geri dönüş sorunlarını kapsar."
                ),
                (
                    "Duyuru ve Tanıtım",
                    "DUYURU",
                    "Belediye duyuruları, tanıtım faaliyetleri ve bilgilendirme içeriklerine ilişkin talepleri kapsar."
                )
            ),

            CreateDepartment(
                "Emlak ve İstimlak Dairesi",
                "EMLK",
                "Belediyeye ait taşınmazlar, kamulaştırma, tahsis, kira ve ecrimisil işlemlerini yürütür.",
                (
                    "Kamulaştırma",
                    "KAMULAS",
                    "Kamulaştırma süreci, bedel ve taşınmaz edinimine ilişkin talepleri kapsar."
                ),
                (
                    "Belediye Taşınmazları",
                    "BLDTAS",
                    "Belediyeye ait arsa, bina ve diğer taşınmazlara ilişkin talepleri kapsar."
                ),
                (
                    "Kira ve Tahsis",
                    "KIRATAH",
                    "Belediye taşınmazlarının kiralanması ve tahsis işlemlerine ilişkin talepleri kapsar."
                ),
                (
                    "Ecrimisil",
                    "ECRIMISIL",
                    "Belediye taşınmazlarının izinsiz kullanımı ve ecrimisil işlemlerine ilişkin talepleri kapsar."
                )
            ),

            CreateDepartment(
                "Mali Hizmetler Dairesi",
                "MALI",
                "Belediyeye ait borç, tahsilat, vergi, harç, ödeme ve iade işlemlerini yürütür.",
                (
                    "Borç ve Tahsilat",
                    "BORCTAH",
                    "Belediye borcu sorgulama, borç bildirimi ve tahsilat işlemlerini kapsar."
                ),
                (
                    "Vergi ve Harç",
                    "VERGI",
                    "Belediye vergileri, harçlar ve ücretlendirmeye ilişkin talepleri kapsar."
                ),
                (
                    "Ödeme Sorunları",
                    "ODEME",
                    "Çevrim içi veya fiziki ödeme sırasında yaşanan sorunları kapsar."
                ),
                (
                    "İade İşlemleri",
                    "IADE",
                    "Fazla veya hatalı yapılan ödemelerin iade taleplerini kapsar."
                )
            ),

            CreateDepartment(
                "Muhtarlık İşleri Dairesi",
                "MUHT",
                "Muhtarlıklar ile belediye arasındaki koordinasyonu ve mahalle taleplerinin takibini yürütür.",
                (
                    "Muhtarlık Talepleri",
                    "MUHTAL",
                    "Muhtarlıklar tarafından belediyeye iletilen genel hizmet taleplerini kapsar."
                ),
                (
                    "Mahalle İhtiyaçları",
                    "MAHIHT",
                    "Mahalle genelindeki ortak ihtiyaç ve hizmet eksikliklerine ilişkin bildirimleri kapsar."
                ),
                (
                    "Muhtar İletişimi",
                    "MUHILET",
                    "Muhtarların belediye birimleriyle iletişim ve koordinasyon taleplerini kapsar."
                )
            )
        };

        context.Departments.AddRange(departments);

        await context.SaveChangesAsync();
    }

    // CreateDepartment is a helper method to seed Departments and RequestCategories tables in DB
    private static Department CreateDepartment(
        string name,
        string code,
        string description,
        params (string _name, string _code, string _description)[] categories)
    {
        var department = new Department
        {
            Name = name,
            Code = code,
            Description = description,
            IsActive = true
        };

        foreach (var category in categories)
        {
            department.RequestCategories.Add(new RequestCategory
            {
                Name = category._name,
                Code = category._code,
                Description = category._description,
                IsActive = true
            });
        }

        return department;
    }

}