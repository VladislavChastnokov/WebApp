//форма добавления/редактирования учебных заведений
function readyInstForm(name, id) {
    $("#instId").val(id);
    $("#instName").val(name);
    if (id != -1) {
        $("#instModalSubmit").text("Сохранить");
    }
    else {
        $("#instModalSubmit").text("Добавить");
    }
}
//форма добавления/редактирования направлений/специальностей
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
//форма смены пароля
function setId(id) {
    $("#userIdField").val(id);
}
//форма удаления записи
function setRecordForDeletion(type, id) {
    $("#recordType").val(type);
    $("#recordId").val(id);
}
//форма добавления/редактирования модулей дисциплин
function readyModuleForm(id, spec, code, name) {
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
//форма добавления/редактирования дисциплин
function readyDiscForm(id, module, name, sem) {
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
//изменение выбранного семестра на форме добавления/редактирования дисциплин
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
//отметка видов практики на форме добавления/редактирования дисциплин
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
//форма добавления/редактирования студентов
function readyStudentForm(id, spec, lastName, firstName, middleName, kurs) {
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

function addStudent() {
    $.ajax({
        type: "POST",
        url: "/Examination/AddStudent",
        data: { lastName: $("#lastName").val(), firstName: $("#firstName").val(), middleName: $("#middleName").val(), kurs: $("#kurs").val(), spec: $("#studSpec").val() },
        success: function (data) {
            if (data != null) {
                //добавление столбца с ФИО студента во все таблицы
                $("table").each(function () {
                    $("<th>" + data.fio + "</th>").insertBefore($("#" + this.id + " > thead > tr > th.last"));
                })
                data.exams.forEach(ex => {
                    //добавление ячейки среднего балла по студенту
                    if ($("#avgS" + data.id + "-avgDM" + ex.moduleId + "-avgPr" + ex.practiceTypeId).length == 0) {
                        $("<td><div id=\"avgS" + data.id + "-avgDM" + ex.moduleId + "-avgPr" + ex.practiceTypeId + "\" class=\"p-1\"></div></td>").insertBefore($("#DM" + ex.moduleId + "-Pr" + ex.practiceTypeId + " > tfoot > tr > td.last"));
                    }
                    //добавление ячейки с оценкой
                    $("<td id=\"ex" + ex.id + "\"></td>").insertBefore($("#DM" + ex.moduleId + "-Pr" + ex.practiceTypeId + " > tbody > tr#D" + ex.disciplineId + " > td.last"))
                    $.ajax({
                        type: "GET",
                        url: "/Examination/Mark",
                        data: { id: ex.id },
                        success: function (response) {
                            $("#ex"+ex.id).html(response);
                        }
                    });
                });
            }
        },
        fail: function () {
            alert('Ошибка!');
        }
    });
}

function Export() {
    //let wr = new TextEncoder();
    //var data = wr.encode($("#content").html());
    $.ajax({
        type: "post",
        url: "Examination/Export",
        data: { dt: $("#content").html() },
        success: function () { alert("!!!") },
        fail: function () { alert("(((") }
    });
}