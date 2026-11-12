# TareaPedidoSap: correccion de conexion SLD

## Conexion explicita al SLD (1.0.1.8)

Las dos sobrecargas de `ApplicationContext.SetApplication` leen
`appSettings/SLDServer` y lo asignan a DI API antes de `Company.Connect()`.
La clave es obligatoria: si falta o esta vacia se informa un error de
configuracion sin intentar la conexion con un destino predeterminado.

```xml
<add key="SLDServer" value="sapserver:40000" />
```

El puerto `40000` corresponde al SLD. DI API obtiene del SLD la direccion
registrada del License Manager (en el entorno diagnosticado, `sapserver:40002`).
Se conserva el mapeo de sociedades y el valor de `UseTrusted` de cada ruta.

### Compilacion

En Windows con Visual Studio/MSBuild, .NET Framework 4.7.2 Developer Pack
y SAP Business One DI API 10 de 64 bits instalada y registrada:

```powershell
nuget restore .\TareaPedidoSap.sln
msbuild .\TareaPedidoSap.sln /t:Rebuild /p:Configuration=Release /p:Prefer32Bit=false
```

La salida corregida se genera en `TareaPedidoSap/bin/Release`, version `1.0.1.8`.
Los ejecutables y configuraciones historicos de `bin/Debug` y los RAR del
repositorio no incorporan esta correccion.

### Validacion sin SAP

```powershell
powershell.exe -NoProfile -File .\Tests\Run-SldConnectionTests.ps1
```

Las pruebas compilan el archivo real `ApplicationContext.cs` con un sustituto
de SAPbobsCOM. Verifican el destino recibido al conectar, las sociedades,
la autenticacion preservada y el rechazo de configuracion ausente o vacia.
No ejecutan el sincronizador ni se conectan a SAP. Los artefactos de prueba
se generan en una carpeta temporal independiente.

### Despliegue y validacion en el servidor

1. Compilar la version corregida y respaldar la carpeta de la tarea.
2. Evitar que la tarea se ejecute durante el reemplazo del ejecutable.
3. Desplegar los artefactos compilados. Conservar la configuracion propia del
   servidor y agregar `SLDServer` a su `TareaPedidoSap.exe.config`.
   No sobrescribir credenciales, URL de API ni base SQL con los valores del
   repositorio. Confirmar que la base SQL sea la usada por el portal.
4. Validar solo conexion/desconexion a `SBO_GRACO`, `SBOBOLIK` y `SBOESCOCESA`
   bajo la identidad de la tarea antes de procesar pedidos.
5. Conciliar pendientes con `ORDR.U_PEDIDO_K66` para identificar pedidos ya
   creados. Luego validar un pedido controlado y su actualizacion en SQL.
6. Restablecer la programacion y comprobar resultados reales en SAP y SQL.
   El codigo de salida `0x0` de la tarea no acredita sincronizacion por si solo.

Agregar la clave sin desplegar el nuevo ejecutable no corrige la version
anterior. Para revertir, restaurar juntos el ejecutable y su configuracion
respaldados; la version anterior conserva el fallo de conexion diagnosticado.

La evidencia del incidente fue: conexion predeterminada con retorno `-4009`
y conexion a `SBO_GRACO` con SLD explicito con retorno `0`, usando las mismas
credenciales y `UseTrusted = true`. La DLL cargada fue DI API `10.00.311` de
64 bits. Falta validar las otras sociedades y el contexto de la tarea programada.
