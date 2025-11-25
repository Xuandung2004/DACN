$(document).ready(function () {

    const userInfo = JSON.parse(localStorage.getItem("currentUser"));
    const userId = userInfo.id;
    
    const apiUrl = "https://localhost:7109/api/DatHang/LichSu/" + userId;

    loadLichSu();

    function loadLichSu() {
        $.ajax({
            url: apiUrl,
            method: "GET",
            dataType: "json",
            success: function (res) {

                $("#orderList").empty();
                console.log(res);

                if (res.length === 0) {
                    $("#orderList").html(`
                        <p class="stext-111 cl2">Bạn chưa có đơn hàng nào.</p>
                    `);
                    return;
                }

                res.forEach(order => {

                    let productHtml = "";

                    order.sanPhams.forEach(p => {
                        productHtml += `
                            <div class="order-product">
                                <img src="${p.anh}" alt="">
                                <div>
                                    <div class="stext-102">${p.tenSp} (Size ${p.kichThuoc})</div>
                                    <div class="stext-111">SL: ${p.soLuong}</div>
                                    <div class="stext-111">${p.gia.toLocaleString()} ₫</div>
                                </div>
                                <div style="margin-left:auto;" class="stext-111">
                                    ${p.thanhTien.toLocaleString()} ₫
                                </div>
                            </div>
                        `;
                    });

                    const row = `
                        <div class="order-item">
                            <div class="order-header">
                                <b>Đơn hàng #${order.donHangID}</b>
                            </div>
                            <div class="stext-111">
                                Ngày đặt: ${new Date(order.ngayDat).toLocaleString()}
                            </div>
                            <div class="stext-111">
                                Trạng thái: <b>${order.trangThai}</b>
                            </div>

                            <div class="order-products p-t-20">
                                ${productHtml}
                            </div>

                            <div class="stext-111 p-t-15">
                                <b>Tổng tiền: ${order.tongTien.toLocaleString()} ₫</b>
                            </div>
                        </div>
                    `;

                    $("#orderList").append(row);
                });
            },

            error: function () {
                $("#orderList").html(`
                    <p class="stext-111 cl2">Không thể tải lịch sử đơn hàng.</p>
                `);
            }
        });
    }
});
