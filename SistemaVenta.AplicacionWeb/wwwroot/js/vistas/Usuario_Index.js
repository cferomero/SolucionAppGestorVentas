const MODELO_BASE = {
    idUsuario: 0,
    nombre: "",
    correo: "",
    telefono: "",
    idRol: 0,
    esActivo: 1,
    urlFoto: ""
}


// Creamos el evento para definir si el documento ya esta cargado

let tablaData;

$(document).ready(function () {

    fetch("/Usuario/ListaRoles")
        .then(response => {
            return response.ok ? response.json() : Promise.reject(response); // Si la respuesta es OK me devuelve en formato JSON y si no me rechaze la respuesta
        })
        .then(responseJson => {
            if (responseJson.length > 0) {
                responseJson.forEach((item) => {
                    $("#cboRol").append(
                        $("<option>").val(item.idRol).text(item.descripcion)
                    )
                })
            }
        })



    // obtener toda la lista de usuarios, para pintarlos en la tabla
    tablaData = $('#tbdata').DataTable({
        responsive: true,
        "ajax": { // Estamos pasando la peticion por Ajax
            "url": '/Usuario/Lista',
            "type": "GET",
            "datatype": "json"
        },
        "columns": [ // Agregamos en DATA todas las columnas en un objeto de JS
            { "data": "idUsuario", "visible": false, "searchable": false },
            {
                "data": "urlFoto", render: function (data) {
                    // definiendo estilos y forma de la foto
                    return `<img style="height:60px" src=${data} class="rounded mx-auto d-block" />`
                }
            },
            { "data": "nombre" },
            { "data": "correo" },
            { "data": "telefono" },
            { "data": "nombreRol" },
            {
                "data": "esActivo", render: function (data) {
                    if (data == 1)
                        return '<span class="badge badge-info">Activo</span>';
                    else
                        return '<span class="badge badge-danger">No Activo</span>';
                }
            },
            { // botones para editar y eliminar
                "defaultContent": '<button class="btn btn-primary btn-editar btn-sm mr-2"><i class="fas fa-pencil-alt"></i></button>' +
                    '<button class="btn btn-danger btn-eliminar btn-sm"><i class="fas fa-trash-alt"></i></button>',
                "orderable": false,
                "searchable": false,
                "width": "80px"
            }
        ],
        order: [[0, "desc"]], // desde la data 0 (idUsuario)
        dom: "Bfrtip",
        buttons: [ // Configuracion para exportar en excel
            {
                text: 'Exportar Excel',
                extend: 'excelHtml5',
                title: '',
                filename: 'Reporte Usuarios',
                exportOptions: {
                    columns: [2, 3, 4, 5, 6] // Especificando que columnas de la tabla vamos a exportar
                }
            }, 'pageLength'
        ],
        language: {
            url: "https://cdn.datatables.net/plug-ins/1.11.5/i18n/es-ES.json"
        },
    });
});



// FUNCIONES PARA CREAR Y EDITAR USUARIO

 //En caso que el modelo este vacio le pasamos el objeto inicial
function MostrarModal(modelo = MODELO_BASE) {
    $("#txtId").val(modelo.idUsuario);
    $("#txtNombre").val(modelo.nombre);
    $("#txtCorreo").val(modelo.correo);
    $("#txtTelefono").val(modelo.telefono);
    $("#cboRol").val(modelo.idRol == 0 ? $("#cboRol option:first").val() : modelo.idRol);
    $("#cboEstado").val(modelo.esActivo);
    $("#txtFoto").val("");
    $("#imgUsuario").attr("src", modelo.urlFoto);


    // modal
    $("#modalData").modal("show")
}

$("#btnNuevo").click(function () {
    MostrarModal()
})



// ***** FUNCIONALIDAD BOTON GUARDAR *******
$("#btnGuardar").click(function () {
    //debugger;
    const inputs = $("input.input-validar").serializeArray();
    const inputsSinValor = inputs.filter((item) => item.value.trim() == "") // filtrar los inputs que no tengan ningun valor


    // Validar que el valor del campo este completo; y mostrar una notificación
    if (inputsSinValor.length > 0) {
        const mensaje = `Debe completar el campo: "${inputsSinValor[0].name}"`;
        toastr.warning("", mensaje);
        $(`input[name="${inputsSinValor[0].name}"]`).focus();
        return;
    }


    const modelo = structuredClone(MODELO_BASE); // Clonar la estructura del modelo base
    modelo['idUsuario'] = parseInt($("#txtId").val()); // Convirtiendo el idUsuario a INT y pasandolo a idUsuario del modelo
    modelo['nombre']    = $("#txtNombre").val();
    modelo['correo']    = $("#txtCorreo").val();
    modelo['telefono']  = $("#txtTelefono").val();
    modelo['idRol']     = $("#cboRol").val();
    modelo['esActivo'] = $("#cboEstado").val();


    const inputFoto = document.getElementById('txtFoto');

    const formData = new FormData(); // añadimos una nueva instancia de FormData()
    formData.append("foto", inputFoto.files[0]); // insertando el primer elemento
    formData.append("modelo", JSON.stringify(modelo)); // convirtiendo nuestro modelo a un cadena de texto con stringify

    $("#modalData").find("div.modal-content").LoadingOverlay("show");


    // Condicional para crear el usuario inexistente y lo creamos
    if (modelo.idUsuario == 0) {

        // Hacer uso del metodo CREAR en el usuarioController
        fetch("/Usuario/Crear", {
            method: "POST",
            body: formData
        })
            .then(response => {
                $("#modalData").find("div.modal-content").LoadingOverlay("hide");
                return response.ok ? response.json() : Promise.reject(response)
            })
            .then(responseJson => {
                if (responseJson.estado) {
                    tablaData.row.add(responseJson.objeto).draw(false);
                    $("#modalData").modal("hide");
                    swal("Listo", "El usuario fue creado!", "success");
                } else {
                    swal("Lo sentimos", responseJson.mensaje, "error");
                }
            })
    } else { // si el usuario existe lo editamos       
        fetch("/Usuario/Editar", { // Editamos un usuario existente
            method: "PUT",
            body: formData
        })
            .then(response => {
                $("#modalData").find("div.modal-content").LoadingOverlay("hide");
                return response.ok ? response.json() : Promise.reject(response)
            })
            .then(responseJson => {
                if (responseJson.estado) {
                    tablaData.row(filaSeleccionada).data(responseJson.objeto).draw(false);
                    filaSeleccionada = null;
                    $("#modalData").modal("hide");
                    swal("Listo", "El usuario fue actualizado!", "success");
                } else {
                    swal("Lo sentimos", responseJson.mensaje, "error");
                }
            })
    }
})



// ****** FUNCIONALIDAD BOTON EDITAR *******
let filaSeleccionada;

// Validando que me muestre los datos también cuando esté en tamaño responsivo
$("#tbdata tbody").on("click", ".btn-editar", function () {
    if ($(this).closest("tr").hasClass("child")) {
        filaSeleccionada = $(this).closest("tr").prev();
    } else {
        filaSeleccionada = $(this).closest("tr");
    }
    // Mostrar la informacion del usuario en el modal
    const data = tablaData.row(filaSeleccionada).data();
    MostrarModal(data);
})


// ****** FUNCIONALIDAD BOTON ELIMINAR *******
$("#tbdata tbody").on("click", ".btn-eliminar", function () {
    let fila;
    if ($(this).closest("tr").hasClass("child")) {
        fila = $(this).closest("tr").prev();
    } else {
        fila = $(this).closest("tr");
    }
    // Mostrar la informacion del usuario en el modal
    const data = tablaData.row(filaSeleccionada).data();
    swal({
        title: '¿Está seguro de eliminar?',
        text: `Eliminar al usuario "${data.nombre}"`,
        type: 'warning',
        showCancelButton: true,
        confirmButtonClass: 'btn-danger',
        confirmButtonText: 'Si, eliminar',
        cancelButtonText: 'No, cancelar',
        closeOnConfirm: false,
        closeOnCancel: true
    },
        function (respuesta) {
            if (respuesta) {
                $('.showSweetAlert').LoadingOverlay('show');

                fetch(`/Usuario/Eliminar?IdUsuario=${data.idUsuario}`, { // Editamos un usuario existente
                    method: "DELETE"
                })
                    .then(response => {
                        $('.showSweetAlert').LoadingOverlay('hide');
                        return response.ok ? response.json() : Promise.reject(response)
                    })
                    .then(responseJson => {
                        if (responseJson.estado) {
                            tablaData.row(fila).remove().draw();
                            swal("Listo", "El usuario fue eliminado!", "success");
                        } else {
                            swal("Lo sentimos", responseJson.mensaje, "error");
                        }
                    })
            }
        }
    );

})