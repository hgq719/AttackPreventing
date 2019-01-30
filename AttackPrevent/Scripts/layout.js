
//转换日期格式(时间戳转换为datetime格式)
function changeDateFormat(cellval) {
    var dateVal = cellval + "";
    if (cellval != null) {
        var date = new Date(parseInt(dateVal.replace("/Date(", "").replace(")/", ""), 10));
        var month = date.getMonth() + 1 < 10 ? "0" + (date.getMonth() + 1) : date.getMonth() + 1;
        var currentDate = date.getDate() < 10 ? "0" + date.getDate() : date.getDate();

        var hours = date.getHours() < 10 ? "0" + date.getHours() : date.getHours();
        var minutes = date.getMinutes() < 10 ? "0" + date.getMinutes() : date.getMinutes();
        var seconds = date.getSeconds() < 10 ? "0" + date.getSeconds() : date.getSeconds();

        return month + "/" + currentDate + "/" + date.getFullYear()+ " " + hours + ":" + minutes + ":" + seconds + "," + date.getMilliseconds();
    }
}

// 日期月份/天的显示，如果是1位数，则在前面加上'0'
function getFormatDate(arg) {
    if (arg == undefined || arg == '') {
        return '';
    }

    var re = arg + '';
    if (re.length < 2) {
        re = '0' + re;
    }

    return re;
}

function num(obj) {
    obj.value = obj.value.replace(/[^\d.]/g, ""); //清除"数字"和"."以外的字符
    obj.value = obj.value.replace(/^\./g, ""); //验证第一个字符是数字
    obj.value = obj.value.replace(/\.{2,}/g, "."); //只保留第一个, 清除多余的
    obj.value = obj.value.replace(".", "$#$").replace(/\./g, "").replace("$#$", ".");
    //obj.value = obj.value.replace(/^(\-)*(\d+)\.(\d\d).*$/, '$1$2.$3'); //只能输入两个小数
}

function selectMenu() {
    $(".navbar-nav-cm li").removeClass("select");
    var selectName = $("#selectedMenu").val();
    $(".navbar-nav-cm ." + selectName).addClass("select");
}

$(document).ready(function () {
    $(".headerRight-cm").hover(function () {
        
        $(this).children(".divAvatarsHasTop").stop(false, true).slideDown(300);
    }, function () {
        $(this).children(".divAvatarsHasTop").stop(false, true).css("display", "none");
    }
    );

    //$("input[name='startTime']").datepicker({
    //    minView: "day", //  选择时间时，最小可以选择到那层；默认是‘hour’也可用0表示
    //    // language: 'zh-CN', // 语言
    //    autoclose: true, //  true:选择时间后窗口自动关闭
    //    format: 'yyyy-mm-dd hh:00:00', // 文本框时间格式，设置为0,最后时间格式为2017-03-23 17:00:00
    //    todayBtn: true, // 如果此值为true 或 "linked"，则在日期时间选择器组件的底部显示一个 "Today" 按钮用以选择当前日期。
    //    startDate: new Date(),  // 窗口可选时间从今天开始
    //    endDate: new Date()   // 窗口最大时间直至今天
    //});
    $("input[name='startTime']").datetimepicker({
        format: 'mm/dd/yyyy hh:ii',
        autoclose: true,
    });

    $("input[name='endTime']").datetimepicker({
        format: 'mm/dd/yyyy hh:ii',
        autoclose: true,
    });

    $("input[name='startTimeBasic']").datetimepicker({
        format: 'mm/dd/yyyy',
        autoclose: true,
        minView: "month"
    });

    $("input[name='endTimeBasic']").datetimepicker({
        format: 'mm/dd/yyyy',
        autoclose: true,
        minView: "month"
    });

    $("[name='o-iftest']").bootstrapSwitch({
        onText: "Test",
        offText: "NotTest",
        //onColor: "success",
        //offColor: "info",
        //'size': "mini",
    });

    $("[name='o-ifenable']").bootstrapSwitch({
        state: true,
        onText: "Enable",
        offText: "Disable",
        //onColor: "success",
        //offColor: "info",
        //'size': "mini",
    });

    $("[name='o-iftest']").bootstrapSwitch('size', 'mini');
    $("[name='o-ifenable']").bootstrapSwitch('size', 'mini');
    $("[name='o-ifenable']").bootstrapSwitch('state', true);

    $('.input-validation-error').parents('.form-group').addClass('has-error');
    $('.field-validation-error').addClass('text-danger');

    $('input.form-control').change(function () {
        $(this).next('span').removeClass('text-danger');
        $(this).next('span').text('');
        $(this).parents('.form-group').removeClass('has-error');

        $('.errorMessage').text('');
    });

    $('textarea.form-control').change(function () {
        $(this).next('span').removeClass('text-danger');
        $(this).next('span').text('');
        $(this).parents('.form-group').removeClass('has-error');

    });

    //$('#accountName').text($.session.get('UserName'));

    $('.onlyInteger').keyup(function () {
        $(this).val($(this).val().replace(/\D|^0/g, ''));
    }).bind("paste", function () {  //CTR+V事件处理    
        $(this).val($(this).val().replace(/\D|^0/g, ''));
        }).css("ime-mode", "disabled"); //CSS设置输入法不可用

    $('.onlyNumber').keyup(function () {
        num(this);
    }).bind("paste", function () {  //CTR+V事件处理    
        num(this);
        }).css("ime-mode", "disabled"); //CSS设置输入法不可用

    selectMenu();
});

