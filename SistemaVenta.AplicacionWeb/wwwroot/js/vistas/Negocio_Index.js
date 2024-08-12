﻿/* MODULO DEL NEGOCIO */

$(document).ready(function () {

    $(".card-body").LoadingOverlay("show");

    fetch("/Negocio/Obtener")
        .then(response => {
            $(".card-body").LoadingOverlay("hide");
            return response.ok ? response.json() : Promise.reject(response);
        })
        .then(responseJson => {

            console.log(responseJson);

            if (responseJson.estado) {
                const d = responseJson.objeto;

                $("#txtNumeroDocumento").val(d.numeroDocumento);
                $("#txtRazonSocial").val(d.nombre);
                $("#txtCorreo").val(d.correo);
                $("#txtDireccion").val(d.direccion);
                $("#txTelefono").val(d.telefono);
                $("#txtImpuesto").val(d.porcentajeImpuesto);
                $("#txtSimboloMoneda").val(d.simboloMoneda);
                $("#imgLogo").attr("src", d.urlLogo);
            } else {
                swal("Lo sentimos", responseJson.mensaje, "error");
            }
        })
})



/* ----------- FUNCION BOTON GUARDAR ----------- */
$("#btnGuardarCambios").click(function () {
    const inputs = $("input.input-validar").serializeArray();
    const inputsSinValor = inputs.filter((item) => item.value.trim() == "") // filtrar los inputs que no tengan ningun valor


    // Validar que el valor del campo este completo; y mostrar una notificación
    if (inputsSinValor.length > 0) {
        const mensaje = `Debe completar el campo: "${inputsSinValor[0].name}"`;
        toastr.warning("", mensaje);
        $(`input[name="${inputsSinValor[0].name}"]`).focus();
        return;
    }

    const modelo = {
        numeroDocumento: $("#txtNumeroDocumento").val(),
        nombre: $("#txtRazonSocial").val(),
        correo: $("#txtCorreo").val(),
        direccion: $("#txtDireccion").val(),
        telefono: $("#txTelefono").val(),
        porcentajeImpuesto: $("#txtImpuesto").val(),
        simboloMoneda: $("#txtSimboloMoneda").val()
    }

    const inputLogo = document.getElementById('txtLogo');
    const formData = new FormData();

    formData.append('logo', inputLogo.files[0]);
    formData.append('modelo', JSON.stringify(modelo));

    $(".card-body").LoadingOverlay("show");

    // Guardar cambios
    fetch("/Negocio/GuardarCambios", {
        method: "POST",
        body: formData
    })
        .then(response => {
            $("#card-body").LoadingOverlay("hide");
            return response.ok ? response.json() : Promise.reject(response);
        })
        .then(responseJson => {

            if (responseJson.estado) {
                const d = responseJson.objeto;
                $("#imgLogo").attr("src", d.urlLogo);

            } else {
                swal("Lo sentimos", responseJson.mensaje, "error");
            }
        })
})