using AloMoallem.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AloMoallem.Web.Data;

public static class SeedData
{
    public static async Task EnsureSeededAsync(IServiceProvider sp)
    {
        var roleManager = sp.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = sp.GetRequiredService<UserManager<AppUser>>();
        var db = sp.GetRequiredService<AppDbContext>();

        var cfg = sp.GetRequiredService<Microsoft.Extensions.Configuration.IConfiguration>();
        var env = sp.GetRequiredService<Microsoft.AspNetCore.Hosting.IWebHostEnvironment>();
        var seedDemo = cfg.GetValue("Seed:DemoAccounts", env.IsDevelopment());

        // Roles
        foreach (var role in new[] { "Customer", "Artisan", "Admin" })
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }


        // Permanent Owner Admin (Production-safe; credentials from configuration)
        var ownerEnabled = cfg.GetValue("Seed:OwnerAdmin:Enabled", true);
        var ownerEmail = cfg.GetValue<string>("Seed:OwnerAdmin:Email");
        var ownerPassword = cfg.GetValue<string>("Seed:OwnerAdmin:Password");

        if (ownerEnabled && !string.IsNullOrWhiteSpace(ownerEmail) && !string.IsNullOrWhiteSpace(ownerPassword))
        {
            var owner = await userManager.FindByEmailAsync(ownerEmail);
            if (owner == null)
            {
                owner = new AppUser
                {
                    UserName = ownerEmail,
                    Email = ownerEmail,
                    EmailConfirmed = true
                };

                var createOwner = await userManager.CreateAsync(owner, ownerPassword);
                if (createOwner.Succeeded)
                {
                    await userManager.AddToRoleAsync(owner, "Admin");
                }
            }
            else
            {
                // Ensure role is always present even if user existed
                if (!await userManager.IsInRoleAsync(owner, "Admin"))
                    await userManager.AddToRoleAsync(owner, "Admin");
            }
        }

        // Professions (idempotent)
        var allProfessions = new[]
        {
            new Profession { Name = "نجار", Icon="🪚", Description="أعمال خشب وأبواب" },
            new Profession { Name = "نجار أثاث", Icon="🪑", Description="تصنيع وصيانة أثاث" },
            new Profession { Name = "حداد", Icon="🔩", Description="أبواب وشبابيك وحدادة" },
            new Profession { Name = "حداد ألمنيوم", Icon="🪟", Description="أعمال ألمنيوم وشبابيك" },
            new Profession { Name = "فني ألوميتال", Icon="🪟", Description="أبواب وشبابيك ألوميتال" },
            new Profession { Name = "كهربائي", Icon="⚡", Description="تصليح وتمديدات كهرباء" },
            new Profession { Name = "سباك", Icon="🚰", Description="صيانة وتمديدات مياه" },
            new Profession { Name = "دهان", Icon="🎨", Description="دهان وديكور" },
            new Profession { Name = "مبلط", Icon="🧱", Description="تركيب بلاط وسيراميك" },
            new Profession { Name = "عامل بناء", Icon="🏗️", Description="أعمال بناء وترميم" },
            new Profession { Name = "فني جبس بورد", Icon="🧱", Description="أسقف وجدران جبس" },
            new Profession { Name = "فني ديكور", Icon="🏠", Description="ديكور وتشطيبات" },
            new Profession { Name = "فني حجر", Icon="🪨", Description="تركيب حجر طبيعي" },
            new Profession { Name = "فني رخام", Icon="🪨", Description="تركيب رخام وجرانيت" },
            new Profession { Name = "فني لحام", Icon="🧑‍🏭", Description="لحام ومعادن" },
            new Profession { Name = "تركيب أقفال", Icon="🔒", Description="أقفال ومفاتيح" },
            new Profession { Name = "فني تكييف", Icon="❄️", Description="تركيب وصيانة تكييف" },
            new Profession { Name = "فني تبريد", Icon="🧊", Description="ثلاجات وفريزرات" },
            new Profession { Name = "فني صيانة أجهزة", Icon="🧰", Description="صيانة أجهزة منزلية" },
            new Profession { Name = "فني غسالات", Icon="🧺", Description="صيانة غسالات ونشافات" },
            new Profession { Name = "فني أفران", Icon="🔥", Description="صيانة أفران وغاز" },
            new Profession { Name = "فني تمديدات غاز", Icon="🔥", Description="تمديدات غاز وفحص" },
            new Profession { Name = "فني عزل", Icon="🧴", Description="عزل حراري ومائي" },
            new Profession { Name = "فني طاقة شمسية", Icon="☀️", Description="تركيب وصيانة ألواح شمسية" },
            new Profession { Name = "فني مصاعد", Icon="🛗", Description="تركيب وصيانة مصاعد" },
            new Profession { Name = "فني كاميرات مراقبة", Icon="📷", Description="تركيب وصيانة كاميرات" },
            new Profession { Name = "فني شبكات", Icon="🌐", Description="شبكات وإنترنت وتمديدات" },
            new Profession { Name = "فني حواسيب", Icon="💻", Description="صيانة كمبيوتر ولابتوب" },
            new Profession { Name = "فني إلكترونيات", Icon="📟", Description="صيانة لوحات وأجهزة إلكترونية" },
            new Profession { Name = "ميكانيكي سيارات", Icon="🛞", Description="صيانة سيارات" },
            new Profession { Name = "كهربائي سيارات", Icon="🚗", Description="كهرباء سيارات وتشخيص أعطال" },
            new Profession { Name = "سمكري", Icon="🔧", Description="إصلاح هيكل السيارة وسمكرة" },
            new Profession { Name = "فني دهان سيارات", Icon="🚗", Description="دهان وتصليح طلاء" },
            new Profession { Name = "فني تلميع سيارات", Icon="✨", Description="تلميع وحماية طلاء" },
            new Profession { Name = "منجد", Icon="🛋️", Description="تنجيد كنب وكراسي" },
            new Profession { Name = "ستائر ومفروشات", Icon="🪟", Description="تفصيل وتركيب ستائر" },
            new Profession { Name = "عامل زجاج", Icon="🪞", Description="قص وتركيب زجاج" },
            new Profession { Name = "عامل نقل", Icon="🚚", Description="نقل أثاث وبضائع" },
            new Profession { Name = "منظف منازل", Icon="🧹", Description="تنظيف منازل ومكاتب" },
            new Profession { Name = "عامل نظافة", Icon="🧽", Description="نظافة وتعقيم" },
            new Profession { Name = "فني مكافحة حشرات", Icon="🪳", Description="رش ومكافحة حشرات" },
            new Profession { Name = "فني حدائق", Icon="🌿", Description="تنسيق حدائق وري" },
            new Profession { Name = "عامل زراعة", Icon="🌾", Description="خدمات زراعية" },
            new Profession { Name = "خياط", Icon="🧵", Description="خياطة وتعديل ملابس" },
            new Profession { Name = "مصفف شعر رجالي", Icon="💈", Description="حلاقة رجالية" },
            new Profession { Name = "مصففة شعر نسائي", Icon="💇‍♀️", Description="تسريح وتصفيف نسائي" },
            new Profession { Name = "فني مكياج", Icon="💄", Description="مكياج مناسبات" },
            new Profession { Name = "طباخ منزلي", Icon="🍲", Description="طبخ منزلي وولائم" },
            new Profession { Name = "حلواني", Icon="🍰", Description="حلويات ومناسبات" },
            new Profession { Name = "مصور", Icon="📸", Description="تصوير مناسبات" },
            new Profession { Name = "فني صوتيات", Icon="🎛️", Description="أنظمة صوت وسماعات" },
            new Profession { Name = "مترجم", Icon="📝", Description="ترجمة وتدقيق" },
            new Profession { Name = "مصمم جرافيك", Icon="🖌️", Description="تصميم شعارات ومطبوعات" },
            new Profession { Name = "مصمم UI/UX", Icon="🧩", Description="تصميم واجهات وتجربة مستخدم" },
            new Profession { Name = "مبرمج", Icon="🧑‍💻", Description="تطوير مواقع وتطبيقات" },
        };

        foreach (var p in allProfessions)
        {
            if (!await db.Professions.AnyAsync(x => x.Name == p.Name))
                db.Professions.Add(p);
        }
        await db.SaveChangesAsync();

        // Governorates + Neighborhoods (idempotent)
        async Task<int> EnsureGovernorateAsync(string name)
        {
            var existing = await db.Governorates.FirstOrDefaultAsync(g => g.Name == name);
            if (existing != null) return existing.Id;

            var g = new Governorate { Name = name };
            db.Governorates.Add(g);
            await db.SaveChangesAsync();
            return g.Id;
        }

        async Task EnsureNeighborhoodAsync(int govId, string name)
        {
            name = (name ?? "").Trim();
            if (name.Length == 0) return;

            if (!await db.Neighborhoods.AnyAsync(n => n.GovernorateId == govId && n.Name == name))
                db.Neighborhoods.Add(new Neighborhood { GovernorateId = govId, Name = name });
        }

        var aleppoGovId = await EnsureGovernorateAsync("حلب");
        var countrysideGovId = await EnsureGovernorateAsync("ريف حلب");

        var aleppoNeighborhoods = new[]
        {
            "الجميلية","حلب الجديدة","الفرقان","الموكامبو","السليمانية","الأشرفية","هنانو",
            "السكري","الأنصاري","صلاح الدين","سيف الدولة","الحمدانية","الزبدية","الشعار"
        };

        var countrysideNeighborhoods = new[]
        {
            "إعزاز","مارع","الباب","منبج","جرابلس","عفرين","تل رفعت","دارة عزة","الأتارب","السفيرة"
        };

        foreach (var n in aleppoNeighborhoods) await EnsureNeighborhoodAsync(aleppoGovId, n);
        foreach (var n in countrysideNeighborhoods) await EnsureNeighborhoodAsync(countrysideGovId, n);
        await db.SaveChangesAsync();

        // ---- Demo accounts ----

        if (seedDemo)
        {
            // Customer
        var customerEmail = "customer@alomallem.local";
        var customer = await userManager.FindByEmailAsync(customerEmail);
        if (customer is null)
        {
            customer = new AppUser { UserName = customerEmail, Email = customerEmail, EmailConfirmed = true, AccountType = "Customer" };
            await userManager.CreateAsync(customer, "Customer123!");
            await userManager.AddToRoleAsync(customer, "Customer");
        }

        if (!await db.CustomerProfiles.AnyAsync(p => p.UserId == customer.Id))
        {
            var neighId = await db.Neighborhoods.Where(n => n.GovernorateId == aleppoGovId).Select(n => n.Id).FirstAsync();
            db.CustomerProfiles.Add(new CustomerProfile
            {
                UserId = customer.Id,
                FullName = "أحمد خالد",
                GovernorateId = aleppoGovId,
                NeighborhoodId = neighId
            });
            await db.SaveChangesAsync();
        }

        // Artisan
        var artisanEmail = "artisan@alomallem.local";
        var artisanUser = await userManager.FindByEmailAsync(artisanEmail);
        if (artisanUser is null)
        {
            artisanUser = new AppUser { UserName = artisanEmail, Email = artisanEmail, EmailConfirmed = true, AccountType = "Artisan" };
            await userManager.CreateAsync(artisanUser, "Artisan123!");
            await userManager.AddToRoleAsync(artisanUser, "Artisan");
        }

        var artisanProfile = await db.ArtisanProfiles.FirstOrDefaultAsync(p => p.UserId == artisanUser.Id);
        if (artisanProfile is null)
        {
            var neighId = await db.Neighborhoods.Where(n => n.GovernorateId == aleppoGovId).Select(n => n.Id).FirstAsync();

            var primaryProfession = await db.Professions.FirstAsync(p => p.Name == "كهربائي");
            var extraProfession = await db.Professions.FirstAsync(p => p.Name == "فني كاميرات مراقبة");

            artisanProfile = new ArtisanProfile
            {
                UserId = artisanUser.Id,
                DisplayName = "محمد الحسن",
                ProfessionId = primaryProfession.Id,
                GovernorateId = aleppoGovId,
                NeighborhoodId = neighId,
                City = "حلب",
                About = "كهربائي محترف بخبرة 8 سنوات. التزام بالمواعيد وجودة تنفيذ عالية.",
                PhoneNumberPublic = "+963000000000",
                PhotoUrl = "/img/default-avatar.svg",
                AvailableNow = true,
                Rating = 4.9,
                CompletedJobs = 57
            };

            db.ArtisanProfiles.Add(artisanProfile);
            await db.SaveChangesAsync();

            // many-to-many professions
            if (!await db.ArtisanProfileProfessions.AnyAsync(x => x.ArtisanProfileId == artisanProfile.Id && x.ProfessionId == primaryProfession.Id))
                db.ArtisanProfileProfessions.Add(new ArtisanProfileProfession { ArtisanProfileId = artisanProfile.Id, ProfessionId = primaryProfession.Id });

            if (!await db.ArtisanProfileProfessions.AnyAsync(x => x.ArtisanProfileId == artisanProfile.Id && x.ProfessionId == extraProfession.Id))
                db.ArtisanProfileProfessions.Add(new ArtisanProfileProfession { ArtisanProfileId = artisanProfile.Id, ProfessionId = extraProfession.Id });

            // Work photos placeholders (ensure files exist in wwwroot/img)
            db.WorkPhotos.AddRange(
                new WorkPhoto { ArtisanProfileId = artisanProfile.Id, Url = "/img/brand-logo.png" },
                new WorkPhoto { ArtisanProfileId = artisanProfile.Id, Url = "/img/logo.svg" }
            );

            await db.SaveChangesAsync();
        }

            // Admin
        var adminEmail = "admin@alomallem.local";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser is null)
        {
            adminUser = new AppUser { UserName = adminEmail, Email = adminEmail, EmailConfirmed = true, AccountType = "Admin" };
            await userManager.CreateAsync(adminUser, "Admin123");
            await userManager.AddToRoleAsync(adminUser, "Admin");
        }
    }
}
}
