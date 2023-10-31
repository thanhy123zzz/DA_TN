$(document).ready(function () {
    var datas = [
        {
            el: $('.cbNhaCC'),
            placeholder: '-- Nhà cung cấp --',
            url: '/QuanLy/NhapKho/api/nccs'
        }
    ];
    configCb(datas);
    configDate();
    configDateTime();
    formatNumberFloatWithElement($('.input-number-float'));

    $.ajax({
        type: 'POST',
        url: '/QuanLy/NhapKho/api/hhs'
    }).done(function (response) {
        $('#tBodyCtpn').append(getRowPhieuNhapCt());
        configRowPhieuNhapCt($('#tBodyCtpn tr:last'), response);
    });

    $(document).on('click', '.btn-remove-ct', function () {
        var tr = $(this).closest('tr');

        if (!(tr.is(":last-child") && tr.is(":first-child"))) {
            tr.find('select').each(function () {
                if (this.selectize) {
                    this.selectize.destroy();
                }
            });
            tr.remove();
            updateGiaTongTien();
        }
    });
    $(document).on('keyup', 'input[name="Sl"]', function () {
        var tr = $(this).closest('tr');
        var soLuongNhap = parseFloat($(this).inputmask('unmaskedvalue'));
        var donGiaNhap = parseFloat(tr.find('input[name="DonGia"]').inputmask('unmaskedvalue'));

        // thay đổi thành tiền
        if (donGiaNhap != "") {
            tr.find('input.ThanhTien').val(soLuongNhap * donGiaNhap);
            updateGiaTongTien();
        }
    });
    $(document).on('keyup', 'input[name="DonGia"]', function () {
        var tr = $(this).closest('tr');
        var donGiaNhap = parseFloat($(this).inputmask('unmaskedvalue'));
        var soLuongNhap = parseFloat(tr.find('input[name="Sl"]').inputmask('unmaskedvalue'));

        if (soLuongNhap != "") {
            tr.find('input.ThanhTien').val(soLuongNhap * donGiaNhap);
            updateGiaTongTien();
        }
    });

    // thay đổi số lượng nhập và đơn giá nếu thành tiền thay đổi
    $(document).on('keyup', 'input.ThanhTien', function () {
        var tr = $(this).closest('tr');
        var thanhTien = $(this).inputmask('unmaskedvalue');
        var soLuongNhap = tr.find('input[name="Sl"]').inputmask('unmaskedvalue');
        var donGiaNhap = tr.find('input[name="DonGia"]').inputmask('unmaskedvalue');

        // thay đổi thành tiền
        if (donGiaNhap !== "" || soLuongNhap !== "") {
            if (soLuongNhap !== "") {
                tr.find('input[name="DonGia"]').val(thanhTien / soLuongNhap);
                updateGiaTongTien();
                return;
            }
            if (donGiaNhap !== "") {
                tr.find('input[name="Sl"]').val(thanhTien / donGiaNhap);
                updateGiaTongTien();
                return;
            }
        }
    });
    $(document).on('keyup', 'input[name="Cktm"], input[name="Thue"]', function () {
        updateGiaTongTien();
    });

    $('#btnTaoPhieu').on('click', function () {
        var form = document.getElementById('formTaoPhieuNhap');
        if (!form.checkValidity()) {
            form.classList.add('was-validated');
        } else {
            if (!$('#nhaCC').val() || !$('#NgayNhap').val()) {
                if (!$('#nhaCC').val()) {
                    showToast("Chưa chọn nhà cung cấp!", 500);
                }
                else {
                    showToast("Chưa nhập ngày tạo!", 500);
                }
            }
            spinnerBtn($('#btnTaoPhieu'));
            // Lắng nghe sự kiện click trên nút có class "action-button"
            var phieuNhapCts = [];
            var check = true;
            $("#tBodyCtpn tr").each(function () {
                var tr = $(this);
                var idHh = tr.find('select[name="Idhh"]').val();
                if (idHh) {
                    tr.removeClass('bg-danger');
                    var slNhap = tr.find('input[name="Sl"]').inputmask('unmaskedvalue');
                    var chiecKhau = tr.find('input[name="Cktm"]').inputmask('unmaskedvalue');
                    var thueVat = tr.find('input[name="Thue"]').inputmask('unmaskedvalue');
                    if (slNhap) {
                        tr.find('input[name="Sl"]').closest('td').removeClass('bg-danger');

                        var data = getDataFromTr(tr);
                        phieuNhapCts.push(data);
                    } else {
                        if (!slNhap) {
                            tr.find('input[name="Sl"]').closest('td').addClass('bg-danger');
                        }
                        check = false;
                        showBtn($('#btnTaoPhieu'), 'Tạo phiếu');
                    }
                } else {
                    if (!tr.is(":last-child")) {
                        tr.addClass('bg-danger');
                    }
                }
            });
            if (check) {
                
                var phieuNhap = {
                    Idncc: $('#nhaCC').val(),
                    SoHd: $('#soHD').val(),
                    NgayTao: $('#NgayNhap').val(),
                    NgayHd: $('#ngayHD').val(),
                    GhiChu: $('#ghiChu').val(),
                    ChiTietPhieuNhaps: phieuNhapCts,
                }
                console.log(phieuNhap);
                showBtn($('#btnTaoPhieu'), 'Tạo phiếu');
                $.ajax({
                    type: "post",
                    url: "/QuanLy/NhapKho/add-pn",
                    data: JSON.stringify(phieuNhap),
                    contentType: "application/json",
                    success: function (response) {
                        showToast(response.message, response.statusCode);
                        showBtn($('#btnTaoPhieu'), 'Tạo phiếu');
                    },
                    error: function (error) {
                        console.log(error);
                        showBtn($('#btnTaoPhieu'), 'Tạo phiếu');
                    }
                });
            }
        }
    });
});
function updateTableThemPhieuXuat(datas) {
    /*xoaTrangPhieuNhapKho();*/
    datas.forEach(function (data) {
        addRowPhieuNhapCt(data);
    });
}
function getRowPhieuNhapCt() {
    return `<tr>
        <td><select class="form-select form-table" name="Idhh" style="width: 300px;"></select></td>
        <td><input autocomplete="off" class="form-control form-table dvt" type="text" readonly style="min-width: 80px;"/></td>
        <td><select class="form-select form-table" name="SoLo" style="width: 120px;"></select></td>
        <td><input autocomplete="off" class="form-control form-table input-number-float" name="Sl" style="min-width: 80px;"/></td>
        <td><input autocomplete="off" class="form-control form-table input-number-float" name="DonGia" style="min-width: 120px;"/></td>
        <td><input autocomplete="off" class="form-control form-table ThanhTien input-number-float" style="min-width: 140px;"/></td>
        <td><input autocomplete="off" class="form-control form-table input-number-float" name="Cktm" style="min-width: 60px;"/></td>
        <td><input autocomplete="off" class="form-control form-table input-number-float" name="Thue" style="min-width: 60px;"/></td>
        <td><input autocomplete="off" class="form-control form-table date-sort-mask" name="Nsx" style="min-width: 110px;"/></td>
        <td><input autocomplete="off" class="form-control form-table date-sort-mask" name="Hsd" style="min-width: 110px;"/></td>
        <td class='last-td-column'>
            <div class="action justify-content-end">
                <button class="text-danger btn-remove-ct">
                    <i class="lni lni-trash-can"></i>
                </button>
            </div>
        </td>
    </tr>`;
    
}
function configRowPhieuNhapCt(tr, hhs) {
    var cbHangHoa = tr.find('select[name="Idhh"]');
    cbHangHoa.selectize({
        maxOptions: 50,
        valueField: "id",
        labelField: "tenHh",
        searchField: ["tenHh", "maHh"],
        placeholder: '-- Hàng hoá --',
        options: hhs,
        loadThrottle: 400,
        dropdownParent: '#dropdow-show',
        closeAfterSelect: 0,
        onDropdownOpen: function ($dropdown) {
            showDropdownMenu(cbHangHoa, $dropdown);
        },
        onChange: function (value) {
            dropDownHhChange(cbHangHoa, value);
        },
        onFocus: function ($dropdown) {
            $('.my-selectize-2').not(this.$input).each(function () {
                if (this.selectize) {
                    this.selectize.close();
                    this.selectize.blur();
                }
            });
        },
        render: {
            option: function (item, escape) {
                return `<div class="px-2 py-1"><b>[${item.maHh}]</b> - ${item.tenHh}</div>`;
            },
            no_results: function (data, escape) {
                return '<div class="no-results">Không tìm thấy dữ liệu </div>';
            },
            dropdown: function () {
                return `<div style="width: 50vw;"></div>`;
            }
        },
    });
    formatNumberFloatWithElement($('tr .input-number-float'));
    configDateLongMask($('.date-sort-mask'));
}
function dropDownHhChange(cbHangHoa, value) {
    var tr = cbHangHoa.closest('tr');
    var selectize = cbHangHoa[0].selectize;
    if (selectize && value) {
        var options = Object.values(selectize.options);
        var option = options.find(function (item) {
            return item.id == value;
        });
        var soLos = [];
        option.soLos.forEach(function (item) {
            soLos.push({
                soLo: item
            });
        });
        tr.find('input.dvt').val(option.tenDonViTinh);
        var cbSoLo = tr.find('select[name="SoLo"]');

        if (cbSoLo[0].selectize) {
            cbSoLo[0].selectize.clear();
            cbSoLo[0].selectize.clearOptions();
            cbSoLo[0].selectize.addOption(soLos);
        } else {
            cbSoLo.selectize({
                placeholder: "-- Số lô --",
                options: soLos,
                valueField: "soLo",
                labelField: "soLo",
                searchField: ["soLo"],
                create: true,
                dropdownParent: '#dropdow-show',
                closeAfterSelect: 0,
                onDropdownOpen: function ($dropdown) {
                    showDropdownMenu(cbSoLo, $dropdown);
                },
                onFocus: function ($dropdown) {
                    $('.my-selectize-2').not(this.$input).each(function () {
                        if (this.selectize) {
                            this.selectize.close();
                            this.selectize.blur();
                        }
                    });
                },
            });
        }
        if (tr.is(':last-child')) {
            $('#tBodyCtpn').append(getRowPhieuNhapCt());
            configRowPhieuNhapCt($('#tBodyCtpn tr:last'), options);
        }
    }
}
function updateGiaTongTien() {

    var tongTienHang = 0;
    var tongTienCK = 0;
    var tongTienThue = 0;
    var tongTra = 0;
    $('#tBodyCtpn tr').each(function () {
        var tr = $(this);

        var SlxGia = parseFloat(tr.find('input.ThanhTien').inputmask('unmaskedvalue'));

        if (SlxGia) {
            tongTienHang += parseFloat(SlxGia);
        }

        var chiecKhau = parseFloat(tr.find('input[name="Cktm"]').inputmask('unmaskedvalue'));
        var tienChiecKhau = 0;
        if (chiecKhau && SlxGia) {
            tienChiecKhau = ((chiecKhau * SlxGia) / 100);
            tongTienCK += tienChiecKhau;
        }

        var thueVat = parseFloat(tr.find('input[name="Thue"]').inputmask('unmaskedvalue'));
        var tienThue = 0;
        if (thueVat && SlxGia) {
            tienThue = (((SlxGia - tienChiecKhau) * thueVat) / 100);
            tongTienThue += tienThue;
        }
    })
    $('#TienHang').val(tongTienHang);
    $('#TienCK').val(tongTienCK);
    $('#TienThue').val(tongTienThue);
    $('#TienThanhToan').val(tongTienHang - tongTienCK + tongTienThue);
}