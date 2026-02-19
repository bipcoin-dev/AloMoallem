منصة مُعلّم (AloMoallem) — نسخة v2 (إصلاح الأخطاء + هوية بصرية)
--------------------------------------------------------------
تقنية: ASP.NET Core MVC (.NET 8) + Identity + SQLite

حل مشكلة الأخطاء:
- تمت إضافة Microsoft.AspNetCore.Identity.EntityFrameworkCore
- تمت إضافة EFCore.Design

تشغيل:
1) افتح AloMoallem.sln بـ Visual Studio 2022+
2) Restore NuGet Packages (تلقائي غالباً)
3) Run (HTTPS)

حسابات تجريبية:
- حرفي: artisan@alomallem.local / Artisan123!
- زبون: customer@alomallem.local / Customer123!
- مدير: admin@alomallem.local / Admin123!

ميزات مضافة حسب الهوية:
- ألوان: Primary #F97316 و Dark #0F172A
- خط Cairo
- شريط بحث بالصفحة الرئيسية + صفحة نتائج مع فلترة (مدينة/موثق)


v3 إضافات:
- لوحة الحرفي + تعديل بروفايل + رفع صور أعمال
- لوحة الأدمن + توثيق الحرفيين
- دردشة فورية SignalR
- REST API مبدئي للتطبيق: /api/professions , /api/professions/{id}/artisans , /api/artisans/{id}
- تم إضافة شعار الهوية البصرية (PNG) داخل wwwroot/img/brand-logo.png

ملاحظة قاعدة البيانات:
إذا كنت شغّلت نسخة أقدم، احذف ملف alomallem.db لتنعمل الجداول الجديدة تلقائياً.


v4 إضافات (تسجيل):
- عند التسجيل: إذا زبون تظهر معلومات الزبون، وإذا حرفي تظهر معلومات الحرفي
- محافظة قائمة منسدلة: (حلب / ريف حلب)
- بعد اختيار المحافظة تظهر قائمة الأحياء/الأرياف تلقائياً
- API للمواقع: /api/locations/governorates و /api/locations/neighborhoods?governorateId=


v4.2:
- تم توسيع قائمة أحياء حلب وريف حلب حسب اللستة التي أرسلتها (مع منع التكرار).


v5:
- نظام طلبات خدمة: الزبون ينشر طلب ضمن نفس المهنة والمنطقة
- ينشأ Offer لكل حرفي مطابق، وأول حرفي يقبل يُسند له الطلب (Transaction)
- صفحات: /Requests/My و /Requests/Create و /ArtisanRequests/Inbox

مهم: احذف alomallem.db عند الترقية لإضافة الجداول الجديدة.


v6:
- تفعيل تأكيد الإيميل قبل تسجيل الدخول
- إرسال رابط تأكيد + رابط إعادة تعيين كلمة المرور
- نسخة تطوير: الإيميلات تُكتب كملفات HTML داخل App_Data/emails
- صفحات: /Account/ConfirmEmail , /Account/ForgotPassword , /Account/ResetPassword


v10:
- Design Tokens (SCSS) مضافة داخل wwwroot/scss كمصدر للتصميم
- Motion System (data-motion + stagger) عبر wwwroot/js/motion.js
- Realtime Notifications (SignalR) + Bell icon + API
- Wizard Flow لطلب الخدمة (3 خطوات)
- Partial Components (SectionHeader/EmptyState)

مهم: احذف alomallem.db لإضافة جدول الإشعارات.
