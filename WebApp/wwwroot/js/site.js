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
        $("#semesterSelect").hide();
        $("#studyPractice").removeAttr("required");
        $("#workPractice").removeAttr("required");
        $("#discModalSubmit").text("Сохранить");
    }
    else {
        $("#practiceSelect").show();
        $("#semesterSelect").show();
        $("#studyPractice").attr("required", true);
        $("#workPractice").attr("required", true);
        $("#discModalSubmit").text("Добавить");
    }
}

function semSelectChanged(v) {
    if (v == 7) {
        $("#practiceSelect").hide();
        $("#studyPractice").prop("checked", false);
        $("#workPractice").prop("checked", false);
        $("#studyPractice").removeAttr("required");
        $("#workPractice").removeAttr("required");
    }
    else {
        $("#practiceSelect").show();
        $("#studyPractice").attr("required", true);
        $("#workPractice").attr("required", true);
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

function setStudentForm(id, spec, lastName, firstName, middleName, kurs) {
    $("#studId").val(id);
    $("#studSpec").val(spec);
    $("#lastName").val(lastName);
    $("#firstName").val(firstName);
    $("#middleName").val(middleName);
    $("#kurs").val(kurs).change();
    if (id != -1) {
        $("#studentModalSubmit").text("Сохранить");
    }
    else {
        $("#studentModalSubmit").text("Добавить");
    }
}

function selectChanged(target, action, value) {
    $.ajax({
        type: "GET",
        url: "/Examination/" + action,
        data: { Id: value },
        success: function (data) {
            var s = '<option value=""></option>';
            for (var i = 0; i < data.length; i++) {
                s += '<option value="' + data[i].id + '">' + data[i].name + '</option>';
            }
            $("#" + target).html(s);
            $("#" + target).change();
        }
    });
}

function beginEdit(id) {
    $("#edit" + id + " > input").val($("#label" + id).text());
    $("#label" + id).hide();
    $("#edit" + id).show();
    $("#edit" + id + " > input").focus();
}

function endEdit(id, save) {
    if (save) {
        if (!isNaN($("#edit" + id + " > input").val()) || $("#edit" + id + " > input").val() == null) {
            $.ajax({
                type: "POST",
                url: "/Examination/SaveMark",
                data: { Id: id, Mark: $("#edit" + id + " > input").val() },
                success: function (data) {
                    $("#label" + id).text($("#edit" + id + " > input").val());
                    if (data.avgD > 0) {
                        $("#avgD" + data.disc).text(Math.round(data.avgD));
                    }
                    else {
                        $("#avgD" + data.disc).text("");
                    }
                    if (data.avgS > 0) {
                        $("#avgS" + data.id + "-avgDM" + data.dm + "-avgPr" + data.pr).text(Math.round(data.avgS));
                    }
                    else {
                        $("#avgS" + data.id + "-avgDM" + data.dm + "-avgPr" + data.pr).text("");
                    }
                    
                },
                fail: function () {
                    alert('Ошибка!');
                }
            });
        }
        else {
            alert('Введенное значение не является допустимым!');
            $("#edit" + id + " > input").val('');
        }
    }
    $("#label" + id).show();
    $("#edit" + id).hide();
}