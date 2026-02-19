# AloMoallem — نشر على سيرفر عام (Production)

## 1) إعدادات الإنتاج السريعة
- ملف الإعدادات: `src/AloMoallem.Web/appsettings.Production.json`
- بشكل افتراضي:
  - قاعدة البيانات SQLite داخل `App_Data/alomallem.db`
  - تعطيل حسابات التجربة في الإنتاج: `Seed:DemoAccounts=false`
  - تعطيل إلزام تأكيد البريد: `Auth:RequireConfirmedAccount=false`

> إذا بدك حسابات التجربة حتى بالإنتاج (غير موصى):
> غيّر `Seed:DemoAccounts` إلى `true` داخل appsettings.Production.json

## 2) أمر النشر
من داخل مجلد المشروع:

```bash
cd src/AloMoallem.Web

dotnet restore

dotnet publish -c Release -o ./publish
```

سيتم إنشاء ملفات النشر داخل:
`src/AloMoallem.Web/publish`

## 3) تشغيل على Linux (Systemd)
انسخ مجلد `publish` إلى السيرفر، ثم:

```bash
export ASPNETCORE_ENVIRONMENT=Production
export ASPNETCORE_URLS=http://127.0.0.1:5000

./AloMoallem.Web
```

يفضل استخدام Nginx كـ reverse proxy مع SSL.

## 4) تشغيل على Windows (IIS)
- Publish إلى Folder.
- انشر المجلد على IIS.
- فعّل Hosting Bundle الخاص بـ .NET.
- تأكد أن `ASPNETCORE_ENVIRONMENT=Production`.

## 5) فحص الصحة
بعد التشغيل:
- افتح: `/health`
لازم يرجع 200 OK إذا DB شغالة.

## 6) ملاحظة مهمة للصور
إذا كنت تخزن صور الأعمال على السيرفر:
- ضع مجلد رفع الصور على Storage ثابت (Volume) + Backup.
