V?i ph?n c�i d?t:
H? th?ng website d� du?c public d? ch?y y�u c?u host v� c?u h�nh database nhu sau:
1. m? file web.config trong thu m?c SETUP/EzzShop/Web.config. Thay connectionstring b?ng du?ng d?n t?i database c?a m�y
....
  <connectionStrings>
    <add name="MultiShop" connectionString="Data Source=GHOST-PC;Initial Catalog=hocaspnet;Integrated Security=True;MultipleActiveResultSets=True;Application Name=EntityFramework" providerName="System.Data.sqlClient" />
    <add name="hocaspnetEntities1" connectionString="metadata=res://*/Models.EzShopV20.EzzModel.csdl|res://*/Models.EzShopV20.EzzModel.ssdl|res://*/Models.EzShopV20.EzzModel.msl;provider=System.Data.SqlClient;provider connection string=&quot;Data Source=GHOST-PC;Initial Catalog=hocaspnet;Integrated Security=True;MultipleActiveResultSets=True;Application Name=EntityFramework&quot;" providerName="System.Data.EntityClient" />
  </connectionStrings>
...
2. Build l�n host
H? th?ng ph?n m?m
1. m? file App.config trong thu m?c C:\Program Files\RateMatrix\RateMatrix. Thay connectionstring b?ng du?ng d?n t?i database c?a m�y
....
   <connectionStrings>
    <add name="hocaspnetEntities" connectionString="metadata=res://*/xxx.EzzModel.csdl|res://*/xxx.EzzModel.ssdl|res://*/xxx.EzzModel.msl;provider=System.Data.SqlClient;provider connection string=&quot;Server=.;Database=hocaspnet;User Id=sa;Password=123456a@;MultipleActiveResultSets=True;App=EntityFramework&quot;" providerName="System.Data.EntityClient"/>
    </connectionStrings>
...
2. Ch?y file RateMatrix.exe