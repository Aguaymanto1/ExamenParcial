Creamos proyecto con:
dotnet new mvc --auth Individual  
luego instalamos depependencias:
  dotnet add package Microsoft.EntityFrameworkCore                      
  dotnet add package Microsoft.EntityFrameworkCore.SqlServer
  dotnet add package Microsoft.EntityFrameworkCore.Tools
  dotnet add package Microsoft.AspNetCore.Identity.EntityFrameworkCore  
  dotnet add package Microsoft.AspNetCore.Identity.UI
  dotnet tool install --global dotnet-aspnet-codegenerator --version 9.0.0
  dotnet add package Microsoft.VisualStudio.Web.CodeGeneration.Design --version 9.0.0
Creamos la base de datos y usamos los siguientes comandos:
  dotnet ef migrations add primeramigra
  dotnet ef database update
Usamos este comando para iniciar proyecto:
  dotnet build
  dotnet  watch run
