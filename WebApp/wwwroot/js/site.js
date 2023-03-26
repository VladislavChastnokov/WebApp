//заполнение формы добавления/редактирования учебных заведений
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
//заполнение формы добавления/редактирования направлений/специальностей
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
//установка значения поля
function setField(name, value) {
    $(name).val(value);
}
//заполнение формы удаления записи
function setRecordForDeletion(type, id) {
    $("#recordType").val(type);
    $("#recordId").val(id);
}
//заполнение формы добавления/редактирования модулей дисциплин
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
//заполнение формы добавления/редактирования дисциплин
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
//заполнение формы добавления/редактирования студентов
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
//Заполнение списков специальностей и семестров на форме формирования страницы успевамости
function selectChanged(target, action, value) {
    //вызов действия контроллера
    $.ajax({
        type: "GET",
        url: "/Examination/" + action,
        data: { Id: value },
        //обработка результата
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
//функция перехода в режим редактирования оценки
function beginEdit(id) {
    $("#edit" + id + " > input").val($("#label" + id).text());
    $("#label" + id).hide();
    $("#edit" + id).show();
    $("#edit" + id + " > input").focus();
}
//функция сохранения изменений оценки и обновления таблиц успеваемости
function endEdit(id) {
    //проверка на числовое или пустое значение
    if (!isNaN($("#edit" + id + " > input").val()) || $("#edit" + id + " > input").val() == null) {
        //вызов действия контроллера
        $.ajax({
            type: "POST",
            url: "/Examination/SaveMark",
            data: { Id: id, Mark: $("#edit" + id + " > input").val() },
            //обработка результата
            success: function (data) {
                $("#label" + id).text($("#edit" + id + " > input").val());
                //обновление ячейки среднего балла по дисциплине
                if (data.avgD > 0) {
                    $("#avgD" + data.disc).text(Math.round(data.avgD));
                }
                else {
                    $("#avgD" + data.disc).text("");
                }
                //обновление ячейки среднего балла по студенту
                if (data.avgS > 0) {
                    $("#avgS" + data.id + "-avgDM" + data.dm + "-avgPr" + data.pr).text(Math.round(data.avgS));
                }
                else {
                    $("#avgS" + data.id + "-avgDM" + data.dm + "-avgPr" + data.pr).text("");
                }

            },
            //обработка ошибки при вызове
            fail: function () {
                alert('Ошибка!');
            }
        });
    }
    else {
        alert('Введенное значение не является допустимым!');
        $("#edit" + id + " > input").val('');
    }
    $("#label" + id).show();
    $("#edit" + id).hide();
}
//функция добавления студента в таблице успеваемости
function addStudent() {
    //вызов действия контроллера
    $.ajax({
        type: "POST",
        url: "/Examination/AddStudent",
        data: { lastName: $("#lastName").val(), firstName: $("#firstName").val(), middleName: $("#middleName").val(), kurs: $("#kurs").val(), spec: $("#studSpec").val() },
        //обработка результата
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
                    $("<td id=\"ex" + ex.id + "\"></td>").insertBefore($("#DM" + ex.moduleId + "-Pr" + ex.practiceTypeId + " > tbody > tr#D" + ex.disciplineId + " > td.last"));
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
        //обработка ошибки при вызове
        fail: function () {
            alert('Ошибка!');
        }
    });
}