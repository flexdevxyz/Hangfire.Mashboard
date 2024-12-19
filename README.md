# Hangfire.Mashboard
Hangfire Dashboard where middleware fetches JobStorage from DI
on every `InvokeAsync`.

## Usage

- net8
- aspnetcore
- Hangfire.Core v1.8.17

Program.cs

```cs
    builder.Services.AddHangfire(_ => { });

    // ...

    var options = app.Services.GetService<DashboardOptions>();
    app.UseHangfireMashboard("/dashboard", options);
```

