var idModel = 0;
$(document).on('click', '#tBodyVaiTro tr', function () {
    // loại bỏ lớp active từ tất cả các hàng
    $('#tBodyVaiTro tr').removeClass('table-active');
    // thêm lớp active cho hàng được nhấp vào
    $(this).addClass('table-active');

    var id = this.getAttribute('data-id')

    $.ajax({
        type: "post",
        url: "/HeThong/PhanQuyen/load-chucnang",
        data: "idPQ=" + id,
        success: function (result) {
            $('#tBodyCN').replaceWith(result);
        },
        error: function (loi) {
            console.log(loi);
        }
    });
});
function showModalVaiTro(id) {
    idModel = id;
    $.ajax({
        type: "get",
        url: "/HeThong/PhanQuyen/show-modal-vaitro/" + id,
        success: function (result) {
            $('#contentModal').empty();
            $('#contentModal').append(result.view);
            $('#staticBackdropLabel').text(result.title);
        },
        error: function (loi) {
            console.log(loi);
        }
    });
};
$('#btnModal').on('click', (e) => {
    $.ajax({
        type: "post",
        url: "/HeThong/PhanQuyen/update-vaitro",
        data: "idVT=" + $('#roles').val(),
        success: function (result) {
            if (result.statusCode == 200) {
                $('#staticBackdrop').modal('hide');
                $('#tBodyVaiTro').empty();

                result.data.map(item => {
                    let isDisabled = result.xoa === false; // kiểm tra giá trị của result.xoa
                    let disabledAttr = isDisabled ? "disabled" : ""; // sử dụng toán tử ba ngôi để đặt giá trị cho thuộc tính
                    $('#tBodyVaiTro').append(`<tr data-id="${item.id}" style="cursor:pointer"><td>${item.idvtNavigation.tenVaiTro}</td><td><div class="action justify-content-end">
                <button ${disabledAttr} class="text-danger" onclick="deletePhanQuyen(${item.id},event)">
                    <i class="lni lni-trash-can"></i>
                </button>
              </div></td></tr>`);
                });

                $('#toast').addClass(result.color);
                $('#toastContent').text(result.message);
                $('#toast').show();

                setTimeout(function () {
                    $('#toast').hide();
                    $('#toast').removeClass(result.color);
                }, 5000);
            }
        },
        error: function (loi) {
            console.log(loi);
        }
    });
});
function deletePhanQuyen(id, e) {
    if (confirm("Bạn có muốn thực hiện thao tác này?")) {
        $.ajax({
            type: "post",
            url: "/HeThong/PhanQuyen/delete-vaitro",
            data: "idVT=" + id,
            success: function (result) {
                if (result.statusCode == 200) {
                    $('#tBodyVaiTro').empty();

                    result.data.map(item => {
                        let isDisabled = result.xoa === false; // kiểm tra giá trị của result.xoa
                        let disabledAttr = isDisabled ? "disabled" : ""; // sử dụng toán tử ba ngôi để đặt giá trị cho thuộc tính
                        $('#tBodyVaiTro').append(`<tr data-id="${item.id}" style="cursor:pointer"><td>${item.idvtNavigation.tenVaiTro}</td><td><div class="action justify-content-end">
                        <button ${disabledAttr} class="text-danger" onclick="deletePhanQuyen(${item.id},event)">
                            <i class="lni lni-trash-can"></i>
                        </button>
                      </div></td></tr>`);
                    });

                    $('#toast').addClass(result.color);
                    $('#toastContent').text(result.message);
                    $('#toast').show();

                    setTimeout(function () {
                        $('#toast').hide();
                        $('#toast').removeClass(result.color);
                    }, 5000);

                    $('#tBodyCN').empty();
                }
            },
            error: function (loi) {
                console.log(loi);
            }
        });
    }
    e.stopPropagation();
}
$('#searchVT').on('keyup', (e) => {
    var key = e.target.value;

    $.ajax({
        type: "post",
        url: "/HeThong/PhanQuyen/search-vaitro",
        data: "key=" + key,
        success: function (result) {
            $('#tBodyVaiTro').empty();
            result.data.map(item => {
                let isDisabled = result.xoa === false; // kiểm tra giá trị của result.xoa
                let disabledAttr = isDisabled ? "disabled" : ""; // sử dụng toán tử ba ngôi để đặt giá trị cho thuộc tính
                $('#tBodyVaiTro').append(`<tr data-id="${item.id}" style="cursor:pointer"><td>${item.idvtNavigation.tenVaiTro}</td><td><div class="action justify-content-end">
                <button ${disabledAttr} class="text-danger" onclick="deletePhanQuyen(${item.id},event)">
                    <i class="lni lni-trash-can"></i>
                </button>
              </div></td></tr>`);
            });

        },
        error: function (loi) {
            console.log(loi);
        }
    });
});
$('#nhap').on('change', (e) => {
    var checkBox = document.getElementById("nhap");
    if (checkBox.checked == true) {
        $('.nhap-check').prop('checked', true);
    } else {
        $('.nhap-check').prop('checked', false);
    }
})
$('#sua').on('change', (e) => {
    var checkBox = document.getElementById("sua");
    if (checkBox.checked == true) {
        $('.sua-check').prop('checked', true);
    } else {
        $('.sua-check').prop('checked', false);
    }
})
$('#xoa').on('change', (e) => {
    var checkBox = document.getElementById("xoa");
    if (checkBox.checked == true) {
        $('.xoa-check').prop('checked', true);
    } else {
        $('.xoa-check').prop('checked', false);
    }
})
$('#tim').on('change', (e) => {
    var checkBox = document.getElementById("tim");
    if (checkBox.checked == true) {
        $('.tim-check').prop('checked', true);
    } else {
        $('.tim-check').prop('checked', false);
    }
})
$('#in').on('change', (e) => {
    var checkBox = document.getElementById("in");
    if (checkBox.checked == true) {
        $('.in-check').prop('checked', true);
    } else {
        $('.in-check').prop('checked', false);
    }
})
$('#xuat').on('change', (e) => {
    var checkBox = document.getElementById("xuat");
    if (checkBox.checked == true) {
        $('.xuat-check').prop('checked', true);
    } else {
        $('.xuat-check').prop('checked', false);
    }
});

$(document).on('change', '.row-check', function () {
    if (this.checked) {
        $(this).closest('tr').find(':checkbox').prop('checked', true);
    } else {
        $(this).closest('tr').find(':checkbox').prop('checked', false);
    }
})
$('#btn-save').on('click', () => {
    if ($('#tBodyCN').has('*').length) {
        let arrPqCn = [];
        $('#tBodyCN tr').each(function () {
            let idPqCn = $(this).data('id-pqcn');
            let idPq = $(this).data('id-pq');
            let idCn = $(this).find('.idCN').data('id-cn');
            let isCheckedNhap = $(this).find('.nhap-check').prop('checked');
            let isCheckedSua = $(this).find('.sua-check').prop('checked');
            let isCheckedXoa = $(this).find('.xoa-check').prop('checked');
            let isCheckedTimKiem = $(this).find('.tim-check').prop('checked');
            let isCheckedIn = $(this).find('.in-check').prop('checked');
            let isCheckedXuat = $(this).find('.xuat-check').prop('checked');
            arrPqCn.push({
                Id: idPqCn,
                IdchucNang: idCn,
                Idpq: idPq,
                Them: isCheckedNhap,
                Sua: isCheckedSua,
                Xoa: isCheckedXoa,
                TimKiem: isCheckedTimKiem,
                In: isCheckedIn,
                Xuat: isCheckedXuat,
            });
        });
        console.log(arrPqCn);
        $.ajax({
            url: '/HeThong/PhanQuyen/save-pqcn',
            method: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(arrPqCn),
            success: function (response) {
                $('#toast').addClass(response.color);
                $('#toastContent').text(response.message);
                $('#toast').show();

                setTimeout(function () {
                    $('#toast').hide();
                    $('#toast').removeClass(response.color);
                }, 5000);
            },
            error: function (error) {
                // Xử lý lỗi nếu cần thiết
            }
        });
    }
});