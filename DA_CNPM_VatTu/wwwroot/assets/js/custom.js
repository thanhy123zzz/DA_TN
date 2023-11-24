// Lấy path của URL hiện tại
var path = window.location.pathname;

// Lấy danh sách các thẻ li trong menu
var menuItems = document.querySelectorAll("ul li.nav-item");

// Duyệt qua từng thẻ li và kiểm tra xem id của thẻ li đó có giống với path không
for (var i = 0; i < menuItems.length; i++) {
    var menuItem = menuItems[i];
    if (menuItem.classList.contains("nav-item-has-children")) {
        for (var j = 0; j < menuItem.querySelector('ul').childElementCount; j++) {

            var menuItemUlA = menuItem.querySelector('ul').children[j].querySelector('a');
            if (path.includes(menuItemUlA.getAttribute("id"))) {
                menuItemUlA.classList.add("active");
                var menuItemA = menuItem.querySelector('a[data-bs-toggle="collapse"]');
                menuItemA.classList.remove("collapsed");
                menuItemA.setAttribute('aria-expanded', 'false');

                var menuItemUl = menuItem.querySelector('ul');
                menuItemUl.classList.add("show");
                menuItemUl.classList.remove("collapsed");
            } else {
                menuItemUlA.classList.remove("active");
            }
        }
    }
    else {
        if (path.includes(menuItem.getAttribute("id"))) {

            // Nếu id của thẻ li đó giống với path thì thêm class "active" vào thẻ li đó
            menuItem.classList.add("active");
        } else {
            menuItem.classList.remove("active");
        }
    }
}

// Get the modal
var modal = document.getElementById("imageModal");

// Get the image and insert it inside the modal - use its "alt" text as a caption
var img = document.getElementsByClassName("image-modal");
var modalImg = document.getElementById("imageHHModal");
var captionText = document.getElementById("imageModelCaption");

$(document).on('click', '.image-modal', function () {
    const imgG = new Image();
    imgG.src = $(this).attr('src');
    imgG.alt = $(this).attr('alt')

    imgG.onload = function () {
        modal.style.display = "block";
        modalImg.src = imgG.src;
        captionText.innerHTML = imgG.alt;
    }
})

// Get the <span> element that closes the modal
var span = document.getElementsByClassName("closeImageModal")[0];

// When the user clicks on <span> (x), close the modal
span.onclick = function () {
    modal.style.display = "none";
}
function configCb(datas) {
    datas.forEach(data => {
        $.ajax({
            url: data.url,
            method: 'POST',
        }).done(function (response) {
            data.el.selectize({
                maxOptions: 50,
                valueField: "id",
                labelField: "ten",
                searchField: ["ten", "ma"],
                placeholder: data.placeholder,
                loadThrottle: 400,
                options: response
            });
        });
    });
}

function configDate() {
    $('.input-date').datetimepicker({
        locale: 'vi',
        useStrict: true,
        format: "DD-MM-yyyy",
        icons: {
            date: "lni lni-calendar",
            up: "lni lni-angle-double-up",
            down: "lni lni-angle-double-down",
            previous: 'lni lni-angle-double-left',
            next: 'lni lni-angle-double-right',
            time: "lni lni-alarm-clock"
        }
    });
}
function configDateTime() {
    $('.input-datetime').datetimepicker({
        locale: 'vi',
        useStrict: true,
        format: "DD-MM-yyyy HH:mm",
        icons: {
            date: "lni lni-calendar",
            up: "lni lni-angle-double-up",
            down: "lni lni-angle-double-down",
            previous: 'lni lni-angle-double-left',
            next: 'lni lni-angle-double-right',
            time: "lni lni-alarm-clock"
        }
    });
}

function showDropdownMenu(select, dropdown) {
    // thẻ của selectize
    var selectize_control = select.next();
    // cài độ dài của dropdow-menu
    /*$('#dropdow-show').css('width', selectize_control.outerWidth(true) + 'px');*/

    // lấy vị trí của thẻ selectize

    $('#dropdow-show').css('top', selectize_control.offset().top + 'px');
    if (document.body.clientWidth > 1200) {
        $('#dropdow-show').css('left', selectize_control.offset().left - 250 + 'px');
    } else {
        $('#dropdow-show').css('left', selectize_control.offset().left + 'px');
    }
    
}
// 100,000,000
function formatNumberWithElement(inputs) {
    inputs.each(function () {
        var value = $(this).val();
        if (value !== "0") {
            $(this).inputmask({
                alias: "numeric",
                groupSeparator: ",",
                autoGroup: true,
                digits: 0,
                allowMinus: false,
                placeholder: '0',
                digitsOptional: false,
                // Định dạng đặc biệt nếu giá trị là 0
                onBeforeMask: function (value, opts) {
                    if (value === "0") {
                        return "0\\";
                    }
                    return value;
                },
            });
        }
    });
}
// 100,000,000.00
function formatNumberFloatWithElement(inputs) {
    inputs.each(function () {
        var min = $(this).attr('min');
        var max = $(this).attr('max');
        var input = $(this).inputmask({
            alias: "numeric",
            radixPoint: ".",
            groupSeparator: ",",
            autoGroup: true,
            digits: 2,
            digitsOptional: true,
            allowMinus: false,
            prefix: "",
            min: min ? parseFloat(min) : 0,
            max: parseFloat(max)
        });
        input.on("blur", function () {
            $(this).trigger('keyup');
        });
    });
}
function configDateShortMask(input) {
    input.inputmask({ alias: "datetime", inputFormat: 'dd-mm-yy', placeholder: '__-__-__' });
}

function configDateLongMask(input) {
    input.inputmask({ alias: "datetime", inputFormat: 'dd-mm-yyyy', placeholder: '__-__-____' });
}

function getDataFromTr(tr) {
    var formData = {};
    tr.find('input, select, textarea').each(function () {
        if (this.name) {
            formData[this.name] = this.value ?? null;
        }
    });

    // Chuyển đối tượng formData thành chuỗi serialized
    return formData;
}
function getDateTimeNow() {
    // Lấy ngày giờ hiện tại
    var currentDate = new Date();

    // Lấy các thành phần ngày, tháng, năm, giờ và phút
    var day = currentDate.getDate();
    var month = currentDate.getMonth() + 1; // Tháng bắt đầu từ 0, cần +1 để đúng
    var year = currentDate.getFullYear();
    var hours = currentDate.getHours();
    var minutes = currentDate.getMinutes();

    // Chuyển đổi thành định dạng "dd-MM-yyyy HH:mm"
    return ("0" + day).slice(-2) + "-" + ("0" + month).slice(-2) + "-" + year + " " + ("0" + hours).slice(-2) + ":" + ("0" + minutes).slice(-2);
}
// 1,000,000
function formatEvenNumber(number) {
    if (number == null) return 0;
    return number.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ",");
}

// 1,000,000.00
function formatOddNumber(number) {
    if (number) {
        if (Number.isInteger(number)) {
            return number.toLocaleString('en-US');
        } else {
            return number.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
        }
    } else {
        return "";
    }
}
function getDateNow() {
    // Lấy ngày giờ hiện tại
    var currentDate = new Date();

    // Lấy các thành phần ngày, tháng và năm
    var day = currentDate.getDate();
    var month = currentDate.getMonth() + 1; // Ghi chú: Tháng trong JavaScript bắt đầu từ 0
    var year = currentDate.getFullYear();

    // Định dạng chuỗi ngày tháng
    return (day < 10 ? '0' : '') + day + '-' + (month < 10 ? '0' : '') + month + '-' + year;
}
function showModalDanger(content) {
    var myModal = new bootstrap.Modal(document.getElementById("modal-danger"), {
    });
    $("#modal-danger-content").text(content);
    myModal.show();
}
var modalDanger = document.getElementById('modal-danger');
if (modalDanger) {
    modalDanger.addEventListener('hidden.bs.modal', function (event) {
        $('#btnModalDanger').off('click');
    });
}

function spinnerBtn(btn) {
    btn.prop('disabled', true);
    btn.html(`<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span>`);
}
function showBtn(btn, text) {
    btn.prop('disabled', false);
    btn.html(text);
}

function showToast(message, statusCode) {
    $('#toastContent').text(message);
    if (statusCode === 200) {
        $('#toast').addClass("bg-success");
        $('#toast').show();
        setTimeout(function () {
            $('#toast').hide();
            $('#toast').removeClass("bg-success");
        }, 5000);
    } else {
        $('#toast').addClass("bg-danger");
        $('#toast').show();

        setTimeout(function () {
            $('#toast').hide();
            $('#toast').removeClass("bg-danger");
        }, 5000);
    }
}
function showLoader(table) {
    table.after(`<div id="loader">
                                        <div class="d-flex justify-content-center">
                                            <div class="spinner-grow text-primary" role="status">
                                                <span class="visually-hidden">Loading...</span>
                                            </div>
                                        </div>
                                    </div>`);
}
function hideLoader() {
    $('#loader').remove();
}
function formatDay(inputString) {
    if (inputString) {
        var inputDate = new Date(inputString);
        var day = inputDate.getDate();
        if (day < 10) {
            day = '0' + day;
        }
        var month = inputDate.getMonth() + 1;
        if (month < 10) {
            month = '0' + month;
        }
        var year = inputDate.getFullYear();
        return day + '-' + month + '-' + year;
    } else {
        return ""
    }
}
function formatDateTime(inputDate) {
    if (inputDate) {
        const date = new Date(inputDate);

        const day = date.getDate().toString().padStart(2, '0');
        const month = (date.getMonth() + 1).toString().padStart(2, '0');
        const year = date.getFullYear().toString();
        const hours = date.getHours().toString().padStart(2, '0');
        const minutes = date.getMinutes().toString().padStart(2, '0');

        return `${day}-${month}-${year} ${hours}:${minutes}`;
    } else {
        return "";
    }
}
function toEmpty(data) {
    if (data == null || data == undefined) {
        return "";
    } else {
        return data;
    }
}
