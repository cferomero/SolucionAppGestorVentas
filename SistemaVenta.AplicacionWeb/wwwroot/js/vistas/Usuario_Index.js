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

// boton nuevo Usuario
$("#btnGuardar").click(function () {
    debugger;
    const inputs = $("input.input-validar").serializeArray();
    const inputsSinValor = inputs.filter((item) => item.value.trim() == "") // filtrar los inputs que no tengan ningun valor
})