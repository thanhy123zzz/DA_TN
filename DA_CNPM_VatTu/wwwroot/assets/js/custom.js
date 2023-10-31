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
                labelField: "tenNcc",
                searchField: ["tenNcc", "maNcc"],
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
        var input = $(this).inputmask({
            alias: "numeric",
            radixPoint: ".",
            groupSeparator: ",",
            autoGroup: true,
            digits: 2,
            digitsOptional: true,
            allowMinus: false,
            prefix: "",
            min: min ? parseFloat(min) : 0
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