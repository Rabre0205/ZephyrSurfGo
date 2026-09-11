# Casos de prueba manuales — Zephyr Surf Go

Esta guía permite verificar los flujos principales antes de integrar una rama o publicar una versión. Marcá cada resultado con `OK`, `ERROR` o `NO PROBADO` y anotá el problema observado.

## Preparación

1. Actualizar la base de datos con los scripts correspondientes a la rama.
2. Iniciar la aplicación y abrirla en una ventana normal y otra privada.
3. Tener disponibles una cuenta de cliente, una de shaper y una de administrador.
4. Preparar dos imágenes JPG o PNG diferentes: una frontal y una trasera, menores a 5 MB.

> No incluyas contraseñas reales, datos bancarios ni credenciales de servicios en este documento o en capturas compartidas.

## A. Acceso y permisos

| ID | Perfil | Pasos | Resultado esperado | Estado |
|---|---|---|---|---|
| A01 | Cliente | Iniciar sesión con correo y contraseña válidos. | Entra al sitio como cliente y puede ver catálogo, carrito y Mis pedidos. | |
| A02 | Shaper | Iniciar sesión como shaper. | Entra al panel del shaper y no al listado general de shapers. | |
| A03 | Admin | Iniciar sesión como administrador. | Entra al panel administrativo y no muestra Configuración de cliente/shaper. | |
| A04 | Visitante | Intentar abrir una URL de Pedidos del shaper. | La aplicación solicita iniciar sesión o rechaza el acceso. | |
| A05 | Cliente | Intentar abrir una URL administrativa. | Recibe acceso denegado; nunca ve información administrativa. | |

## B. Publicación de tablas

| ID | Perfil | Pasos | Resultado esperado | Estado |
|---|---|---|---|---|
| B01 | Shaper | Crear una tabla sin foto frontal. | No permite guardar y explica que la foto frontal es obligatoria. | |
| B02 | Shaper | Crear una tabla sin foto trasera. | No permite guardar y explica que la foto trasera es obligatoria. | |
| B03 | Shaper | Cargar un archivo no permitido o mayor a 5 MB. | Rechaza el archivo y conserva los datos posibles del formulario. | |
| B04 | Shaper | Crear una tabla con datos válidos y ambas fotos. | Guarda una sola tabla y aparece en Productos y en su página pública. | |
| B05 | Shaper | Editar una tabla sin elegir fotos nuevas. | Conserva las fotos existentes. | |
| B06 | Otro shaper | Intentar editar la tabla anterior cambiando el ID en la URL. | Devuelve No encontrado o Acceso denegado. | |

## C. Catálogo y cambio de imagen

| ID | Perfil | Pasos | Resultado esperado | Estado |
|---|---|---|---|---|
| C01 | Cliente | Abrir la página pública de un shaper. | Separa claramente Tablas y Accesorios. | |
| C02 | Cliente/escritorio | Pasar el mouse sobre la foto de una tabla. | Cambia del frente al dorso sin botones. | |
| C03 | Cliente/escritorio | Retirar el mouse de la foto. | Vuelve inmediatamente al frente. | |
| C04 | Cliente/móvil | Tocar o enfocar la foto de una tabla. | Permite consultar el dorso sin desarmar la tarjeta. | |
| C05 | Cliente | Revisar una tabla antigua sin dorso, si existe. | Mantiene el frente y no muestra una imagen rota. | |

## D. Carrito y accesorios

| ID | Perfil | Pasos | Resultado esperado | Estado |
|---|---|---|---|---|
| D01 | Cliente | Agregar una tabla al carrito. | Aparece una vez y el contador coincide con el contenido. | |
| D02 | Cliente | Intentar agregar nuevamente la misma tabla. | No duplica la tabla ni produce una excepción. | |
| D03 | Cliente | Agregar dos unidades de un accesorio con stock. | Actualiza cantidad y subtotal correctamente. | |
| D04 | Cliente | Superar el stock de un accesorio. | Rechaza la cantidad y muestra un mensaje comprensible. | |
| D05 | Cliente | Quitar un producto. | Desaparece y se recalculan contador, subtotal y total. | |

## E. Cuestionario y customizador

| ID | Perfil | Pasos | Resultado esperado | Estado |
|---|---|---|---|---|
| E01 | Cliente | Completar las ocho preguntas. | El progreso muestra 8 pasos y permite avanzar/retroceder sin perder respuestas. | |
| E02 | Cliente | Indicar que no posee tabla actual. | Permite continuar sin exigir medidas de una tabla previa. | |
| E03 | Cliente | Finalizar el cuestionario. | Abre Customiza tu tabla con recomendación y datos iniciales coherentes. | |
| E04 | Cliente | Cambiar forma, medidas, construcción y colores. | El resumen y la vista aproximada se actualizan sin errores. | |
| E05 | Cliente | Alternar Frente y Dorso. | Ambas siluetas tienen el mismo tamaño y muestran el diseño aproximado. | |
| E06 | Cliente | Revisar ambas vistas. | No aparecen quillas, plugs, grip, pad ni parche negro sobre la tabla. | |
| E07 | Cliente | Revisar el customizador completo. | No existe una sección para agregar accesorios. | |
| E08 | Cliente | Enviar una personalización válida. | Crea un solo pedido y abre Mis pedidos; no cobra ni agrega accesorios. | |
| E09 | Cliente | Pulsar rápidamente dos veces Enviar. | El botón se bloquea durante el envío y no crea duplicados. | |

## F. Pedido personalizado y cotización

| ID | Perfil | Pasos | Resultado esperado | Estado |
|---|---|---|---|---|
| F01 | Shaper | Abrir el pedido personalizado recibido. | Ve respuestas, medidas, diseño, colores y notas del cliente. | |
| F02 | Shaper | Cotizar con precio cero o inválido. | No permite guardar la cotización. | |
| F03 | Shaper | Definir un precio válido. | El cliente ve el precio y puede aceptar o rechazar. | |
| F04 | Cliente | Aceptar la cotización. | El estado cambia una sola vez y deja de ofrecer aceptar/rechazar. | |
| F05 | Cliente | Rechazar la cotización. | El pedido queda rechazado y no puede avanzar a preparación. | |
| F06 | Otro cliente | Cambiar el ID de un pedido en la URL. | No puede consultar ni responder un pedido ajeno. | |

## G. Seguimiento

| ID | Perfil | Pasos | Resultado esperado | Estado |
|---|---|---|---|---|
| G01 | Shaper | Informar una fecha estimada y un mensaje. | El cliente ve la fecha, el mensaje y la última actualización. | |
| G02 | Shaper | Cambiar una fecha ya aceptada. | Conserva la fecha original y solicita nuevamente la aceptación cuando corresponda. | |
| G03 | Shaper | Intentar saltar de Pagado a Entregado. | El sistema rechaza el salto de estado. | |
| G04 | Shaper | Avanzar Pagado → En preparación → Enviado/Listo → Entregado. | La barra progresa en orden y el cliente ve cada estado. | |
| G05 | Cliente | Abrir un pedido con fecha vencida. | Muestra claramente que el plazo estimado fue superado. | |

## H. Diseño adaptable y mensajes

| ID | Dispositivo | Pasos | Resultado esperado | Estado |
|---|---|---|---|---|
| H01 | Escritorio | Revisar páginas a 100 % de zoom. | No hay texto cortado, superpuesto ni botones fuera de pantalla. | |
| H02 | Móvil | Revisar a aproximadamente 390 px de ancho. | Formularios y tarjetas forman una sola columna utilizable. | |
| H03 | Teclado | Recorrer controles usando Tab y Enter. | Se distingue el foco y se pueden ejecutar las acciones principales. | |
| H04 | Cualquier | Provocar una validación. | El mensaje indica qué ocurrió y cómo corregirlo. | |

## Registro de ejecución

## I. Reseñas, favoritos y diseños guardados

| ID | Perfil | Pasos | Resultado esperado | Estado |
|---|---|---|---|---|
| I01 | Cliente | Marcar una tabla y un accesorio con “Guardar”. Abrir “Mis favoritos”. | Ambos productos aparecen una sola vez. | |
| I02 | Cliente | Volver a pulsar “Guardada” en un producto. | El producto se elimina de favoritos sin afectar el carrito. | |
| I03 | Cliente | Completar un pedido y abrir su detalle. | Cada producto ofrece “Escribir reseña”. | |
| I04 | Cliente | Publicar entre 1 y 5 estrellas con un comentario. | La reseña aparece en el producto y actualiza su promedio. | |
| I05 | Cliente | Intentar reseñar un pedido no completado, un pedido ajeno o el mismo producto dos veces. | El sistema rechaza la operación. | |
| I06 | Cliente | Personalizar una tabla, pulsar “Guardar diseño” y asignarle un nombre. | Aparece en “Mis diseños guardados”; no crea pedido ni reserva precio. | |
| I07 | Cliente | Abrir “Continuar diseño” desde un borrador. | Regresa al shaper y restaura las opciones y colores guardados. | |
| I08 | Cliente | Eliminar un diseño guardado. | Desaparece de la lista y no altera pedidos existentes. | |
| I09 | Visitante sin sesión | Abrir “Shapers”, pulsar “Sumar mi marca”, completar el formulario y enviarlo. | No exige una cuenta; muestra la confirmación y llega un correo a `zephyrsurfgo@gmail.com` con todos los datos, sin incluir contraseñas. | |
| I10 | Visitante | Enviar la solicitud sin aceptar el contacto o con datos incompletos. | No envía el correo e indica claramente qué falta. | |
| I10b | Visitante sin sesión | Desde la lista pública intentar abrir el perfil completo de un shaper. | Redirige al inicio de sesión y no permite personalizar, guardar favoritos ni comprar. | |
| I11 | Cliente | Abrir “Pedidos e historial” desde el menú del perfil y seleccionar “En curso”. | Solo aparecen compras y pedidos personalizados que todavía requieren seguimiento. | |
| I12 | Cliente | Seleccionar “Historial”. | Aparecen compras entregadas y solicitudes personalizadas finalizadas, rechazadas o no disponibles. | |
| I13 | Cliente | Abrir un elemento del historial. | Puede consultar vendedor, fecha, estado, importe y detalle sin modificar otros pedidos. | |

- Fecha:
- Rama/commit:
- Navegador y versión:
- Base de datos utilizada:
- Persona que realizó la prueba:
- Casos con error:
- Observaciones:

## Ejecución automática

Desde la carpeta raíz del proyecto:

```powershell
dotnet test
```

La prueba es satisfactoria cuando termina con `Con error: 0`. Las pruebas automáticas complementan esta guía; no reemplazan la revisión visual ni los flujos que dependen de una base de datos y servicios externos.
