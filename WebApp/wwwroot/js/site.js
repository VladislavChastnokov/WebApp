// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
function readyForm(name, id) {
    $("#instId").val(id);
    $("#instName").val(name);
    if (id != -1) {
        $("#instModalSubmit").text("Сохранить");
    }
    else {
        $("#instModalSubmit").text("Добавить");
    }
}

function readySpecForm(name, code, id, instId) {
    $("#specId").val(id);
    $("#specInstId").val(instId);
    $("#specCode").val(code);
    $("#specName").val(name);
    if (id != -1) {
        $("#specModalSubmit").text("Сохранить");
    }
    else {
        $("#specModalSubmit").text("Добавить");
    }
}

function deleteConfirmation(type, id) {
    $("#recordId").val(id);
    $("#recordType").val(type);
}

function setId(field, x) {
    $(field).val(x);
}

function setRecordForDeletion(type, id) {
    $("#recordType").val(type);
    $("#recordId").val(id);
}

function setModuleForm(id, spec, code, name) {
    $("#moduleId").val(id);
    $("#specId").val(spec);
    $("#moduleCode").val(code);
    $("#moduleName").val(name);
    if (id != -1) {
        $("#moduleModalSubmit").text("Сохранить");
    }
    else {
        $("#moduleModalSubmit").text("Добавить");
    }
}

function setDiscForm(id, module, name, sem) {
    $("#discId").val(id);
    $("#mId").val(module);
    $("#discName").val(name);
    $("#semester").val(sem).change();
    if (id != -1) {
        $("#practiceSelect").hide();
        $("#studyPractice").removeAttr("required");
        $("#workPractice").removeAttr("required");
        $("#discModalSubmit").text("Сохранить");
    }
    else {
        $("#practiceSelect").show();
        $("#studyPractice").attr("required", true);
        $("#workPractice").attr("required", true);
        $("#discModalSubmit").text("Добавить");
    }
}

function checkBoxClick(x) {
    if (x.checked) {
        $("#studyPractice").removeAttr("required");
        $("#workPractice").removeAttr("required");
    }
    else {
        if (!$("#studyPractice").is(':checked') && !$("#workPractice").is(':checked')) {
            $("#studyPractice").attr("required", true);
            $("#workPractice").attr("required", true);
        }
    }
}