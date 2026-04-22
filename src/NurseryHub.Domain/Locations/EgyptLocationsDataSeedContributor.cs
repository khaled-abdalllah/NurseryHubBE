using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;

namespace NurseryHub.Locations;

public class EgyptLocationsDataSeedContributor : IDataSeedContributor, ITransientDependency
{
    private readonly IRepository<Governorate, Guid> _governorateRepository;
    private readonly IRepository<City, Guid> _cityRepository;

    public EgyptLocationsDataSeedContributor(
        IRepository<Governorate, Guid> governorateRepository,
        IRepository<City, Guid> cityRepository)
    {
        _governorateRepository = governorateRepository;
        _cityRepository = cityRepository;
    }

    public async Task SeedAsync(DataSeedContext context)
    {
        if (await _governorateRepository.GetCountAsync() > 0)
        {
            return;
        }

        var governorates = new List<Governorate>();
        var cities = new List<City>();

        foreach (var item in EgyptData)
        {
            var governorateId = Guid.NewGuid();
            governorates.Add(new Governorate(governorateId, item.Code, item.NameEn, item.NameAr));

            var cityIndex = 1;
            foreach (var city in item.Cities)
            {
                var cityId = Guid.NewGuid();
                cities.Add(new City(cityId, governorateId, $"{item.Code}-{cityIndex:00}", city.NameEn, city.NameAr));
                cityIndex++;
            }
        }

        await _governorateRepository.InsertManyAsync(governorates, autoSave: true);
        await _cityRepository.InsertManyAsync(cities, autoSave: true);
    }

    private static readonly IReadOnlyList<EgyptGovernorateSeedItem> EgyptData = new List<EgyptGovernorateSeedItem>
    {
        new("EG-C", "Cairo", "القاهرة", new List<CitySeedItem>
        {
            new("Cairo", "القاهرة"),
            new("Heliopolis", "مصر الجديدة"),
            new("Nasr City", "مدينة نصر"),
            new("Maadi", "المعادي")
        }),
        new("EG-GZ", "Giza", "الجيزة", new List<CitySeedItem>
        {
            new("Giza", "الجيزة"),
            new("6th of October", "السادس من أكتوبر"),
            new("Sheikh Zayed", "الشيخ زايد"),
            new("Hawamdeya", "الحوامدية")
        }),
        new("EG-ALX", "Alexandria", "الإسكندرية", new List<CitySeedItem>
        {
            new("Alexandria", "الإسكندرية"),
            new("Borg El Arab", "برج العرب"),
            new("El Agamy", "العجمي")
        }),
        new("EG-DK", "Dakahlia", "الدقهلية", new List<CitySeedItem>
        {
            new("Mansoura", "المنصورة"),
            new("Mit Ghamr", "ميت غمر"),
            new("Talkha", "طلخا"),
            new("Belqas", "بلقاس")
        }),
        new("EG-BH", "Beheira", "البحيرة", new List<CitySeedItem>
        {
            new("Damanhur", "دمنهور"),
            new("Kafr El Dawwar", "كفر الدوار"),
            new("Rosetta", "رشيد"),
            new("Edku", "إدكو")
        }),
        new("EG-MN", "Minya", "المنيا", new List<CitySeedItem>
        {
            new("Minya", "المنيا"),
            new("Mallawi", "ملوي"),
            new("Samalut", "سمالوط"),
            new("Beni Mazar", "بني مزار")
        }),
        new("EG-KN", "Qena", "قنا", new List<CitySeedItem>
        {
            new("Qena", "قنا"),
            new("Qus", "قوص"),
            new("Nag Hammadi", "نجع حمادي")
        }),
        new("EG-ASN", "Aswan", "أسوان", new List<CitySeedItem>
        {
            new("Aswan", "أسوان"),
            new("Kom Ombo", "كوم أمبو"),
            new("Edfu", "إدفو")
        }),
        new("EG-LX", "Luxor", "الأقصر", new List<CitySeedItem>
        {
            new("Luxor", "الأقصر"),
            new("Esna", "إسنا"),
            new("Armant", "أرمنت")
        }),
        new("EG-SOH", "Sohag", "سوهاج", new List<CitySeedItem>
        {
            new("Sohag", "سوهاج"),
            new("Akhmim", "أخميم"),
            new("Tahta", "طهطا"),
            new("Girga", "جرجا")
        }),
        new("EG-ASY", "Asyut", "أسيوط", new List<CitySeedItem>
        {
            new("Asyut", "أسيوط"),
            new("Dairut", "ديروط"),
            new("Manfalut", "منفلوط"),
            new("Abnub", "أبنوب")
        }),
        new("EG-FYM", "Faiyum", "الفيوم", new List<CitySeedItem>
        {
            new("Faiyum", "الفيوم"),
            new("Ibshaway", "إبشواي"),
            new("Senuris", "سنورس"),
            new("Tamiya", "طامية")
        }),
        new("EG-BNS", "Beni Suef", "بني سويف", new List<CitySeedItem>
        {
            new("Beni Suef", "بني سويف"),
            new("Al Wasta", "الواسطى"),
            new("Nasser", "ناصر"),
            new("Ehnasia", "إهناسيا")
        }),
        new("EG-MNF", "Monufia", "المنوفية", new List<CitySeedItem>
        {
            new("Shibin El Kom", "شبين الكوم"),
            new("Menouf", "منوف"),
            new("Ashmoun", "أشمون"),
            new("El Bagour", "الباجور")
        }),
        new("EG-SHR", "Sharqia", "الشرقية", new List<CitySeedItem>
        {
            new("Zagazig", "الزقازيق"),
            new("Belbeis", "بلبيس"),
            new("10th of Ramadan", "العاشر من رمضان"),
            new("Abu Kabir", "أبو كبير")
        }),
        new("EG-GHB", "Gharbia", "الغربية", new List<CitySeedItem>
        {
            new("Tanta", "طنطا"),
            new("El Mahalla El Kubra", "المحلة الكبرى"),
            new("Kafr El Zayat", "كفر الزيات"),
            new("Zefta", "زفتى")
        }),
        new("EG-KFS", "Kafr El Sheikh", "كفر الشيخ", new List<CitySeedItem>
        {
            new("Kafr El Sheikh", "كفر الشيخ"),
            new("Desouk", "دسوق"),
            new("Fuwa", "فوه"),
            new("Baltim", "بلطيم")
        }),
        new("EG-DM", "Damietta", "دمياط", new List<CitySeedItem>
        {
            new("Damietta", "دمياط"),
            new("New Damietta", "دمياط الجديدة"),
            new("Ras El Bar", "رأس البر"),
            new("Kafr Saad", "كفر سعد")
        }),
        new("EG-IS", "Ismailia", "الإسماعيلية", new List<CitySeedItem>
        {
            new("Ismailia", "الإسماعيلية"),
            new("Fayed", "فايد"),
            new("Qantara East", "القنطرة شرق"),
            new("Qantara West", "القنطرة غرب")
        }),
        new("EG-SUZ", "Suez", "السويس", new List<CitySeedItem>
        {
            new("Suez", "السويس"),
            new("Ain Sokhna", "العين السخنة"),
            new("Arbaeen", "الأربعين")
        }),
        new("EG-PTS", "Port Said", "بورسعيد", new List<CitySeedItem>
        {
            new("Port Said", "بورسعيد"),
            new("Port Fouad", "بورفؤاد")
        }),
        new("EG-SIN", "North Sinai", "شمال سيناء", new List<CitySeedItem>
        {
            new("Arish", "العريش"),
            new("Sheikh Zuweid", "الشيخ زويد"),
            new("Rafah", "رفح"),
            new("Bir El Abd", "بئر العبد")
        }),
        new("EG-SSIN", "South Sinai", "جنوب سيناء", new List<CitySeedItem>
        {
            new("El Tor", "الطور"),
            new("Sharm El Sheikh", "شرم الشيخ"),
            new("Dahab", "دهب"),
            new("Nuweiba", "نويبع")
        }),
        new("EG-BA", "Red Sea", "البحر الأحمر", new List<CitySeedItem>
        {
            new("Hurghada", "الغردقة"),
            new("Safaga", "سفاجا"),
            new("Marsa Alam", "مرسى علم"),
            new("Quseir", "القصير")
        }),
        new("EG-WAD", "New Valley", "الوادي الجديد", new List<CitySeedItem>
        {
            new("Kharga", "الخارجة"),
            new("Dakhla", "الداخلة"),
            new("Farafra", "الفرافرة")
        }),
        new("EG-MT", "Matrouh", "مطروح", new List<CitySeedItem>
        {
            new("Marsa Matruh", "مرسى مطروح"),
            new("El Alamein", "العلمين"),
            new("Siwa", "سيوة"),
            new("Sallum", "السلوم")
        }),
        new("EG-QB", "Qalyubia", "القليوبية", new List<CitySeedItem>
        {
            new("Benha", "بنها"),
            new("Shubra El Kheima", "شبرا الخيمة"),
            new("Qalyub", "قليوب"),
            new("Obour", "العبور")
        })
    };

    private sealed record EgyptGovernorateSeedItem(
        string Code,
        string NameEn,
        string NameAr,
        IReadOnlyList<CitySeedItem> Cities);

    private sealed record CitySeedItem(
        string NameEn,
        string NameAr);
}
