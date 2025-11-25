// =========================
// ⚙️ Cấu hình API
// =========================
const diaChiBaseUrl = 'http://localhost:5150/api/DiaChi'; 

// =========================
// 🔧 Hàm tiện ích (Giữ nguyên)
// =========================
async function fetchJson(url, options = {}) {
    const res = await fetch(url, options);
    const text = await res.text();
    if (!res.ok) {
        let message = text;
        try {
            const json = JSON.parse(text);
            message = json.message || JSON.stringify(json);
        } catch {}
        throw new Error(message);
    }
    return text ? JSON.parse(text) : {};
}

function showToast(message, type = "success") {
    if (typeof $ === 'undefined' || typeof $.fn.toast === 'undefined') {
        console.error("Lỗi: jQuery hoặc Bootstrap JS (Toast) chưa được tải.");
        console.log(`[Toast ${type}]: ${message}`); 
        return;
    } 
    
    const bgClass = type === "success" ? "bg-success" : (type === "error" ? "bg-danger" : "bg-info");
    const title = type === "success" ? "Thành công" : (type === "error" ? "Lỗi" : "Thông báo");
    
    const html = `
        <div class="toast border-0 ${bgClass} text-white" role="alert" aria-live="assertive" aria-atomic="true" data-delay="3000">
            <div class="toast-header">
                <strong class="mr-auto text-dark">${title}</strong>
                <button type="button" class="ml-2 mb-1 close" data-dismiss="toast" aria-label="Close">
                    <span aria-hidden="true">&times;</span>
                </button>
            </div>
            <div class="toast-body">
                ${message}
            </div>
        </div>
    `;

    $("#toastContainer").append(html); 
    $("#toastContainer .toast").last().toast('show'); 
    $("#toastContainer .toast").last().on('hidden.bs.toast', function () {
        $(this).remove();
    });
}

// 🔑 Hàm lấy thông tin người dùng (GIẢ ĐỊNH)
function getUserId() {
    const userInfo = JSON.parse(localStorage.getItem("currentUser"));
    const userId = userInfo ? userInfo.id : null; // Xử lý nếu currentUser là null
    return userId;
}

function getAuthToken() {
    // Chỉ để chuỗi Token (bao gồm "Bearer " và token JWT)
    return 'Bearer ' + 'TOKEN_THUC_TE_CUA_BAN'; 
}

// =========================
// ➕ Thêm địa chỉ mới (POST)
// =========================
async function themDiaChi(event) {
    event.preventDefault(); 
    
    if (typeof $ === 'undefined') {
        return showToast("Lỗi: Thư viện jQuery chưa được tải.", "error");
    }

    const userId = getUserId();
    if (!userId) {
        return showToast("Lỗi: Không tìm thấy ID người dùng. Vui lòng đăng nhập lại.", "error");
    }
    const token = getAuthToken();

    // Lấy dữ liệu từ form modal
    const tenNguoiNhan = $("#TenNguoiNhan").val().trim();
    const sdt = $("#Sdt").val().trim();
    const diaChiCuThe = $("#DiaChiCuThe").val().trim();

    // Validation
    if (!tenNguoiNhan || !sdt || !diaChiCuThe) {
        return showToast("Vui lòng điền đủ Tên người nhận, SĐT và Địa chỉ cụ thể.", "error");
    }

    // Payload chuẩn bị gửi lên API (Khớp với DiaChiCreateModel trong C#)
    const payload = { 
        IdNguoiDung: userId, 
        TenNguoiNhan: tenNguoiNhan, 
        Sdt: sdt, 
        DiaChiCuThe: diaChiCuThe 
    };

    try {
        // Thực hiện request POST đến API DiaChi
        const result = await fetchJson(`${diaChiBaseUrl}`, {
            method: 'POST',
            headers: { 
                'Authorization': token,
                'Content-Type': 'application/json' 
            },
            body: JSON.stringify(payload)
        });

        showToast(result.message || "Thêm địa chỉ thành công!", "success");
        
        // Đóng modal và reset form
        $("#themDiaChiModal").modal('hide');
        $("#themDiaChiForm")[0].reset(); 
        
        // Tải lại danh sách địa chỉ để thấy địa chỉ mới
        await loadAddresses(); 

    } catch (err) {
        // Hiển thị lỗi từ server
        showToast(`Thêm địa chỉ thất bại: ${err.message}`, "error");
    }
}

// =========================
// ⚡ Gắn sự kiện 
// =========================
function gắnSựKiệnThemDiaChi() {
    const addButton = document.getElementById('themDiaChiBtn');
    if (addButton) {
        // Gắn sự kiện click để mở modal
        addButton.addEventListener('click', () => {
            if (typeof $ !== 'undefined') {
                $("#themDiaChiModal").modal('show');
            } else {
                showToast("Lỗi: Bootstrap JS (Modal) chưa được tải.", "error");
            }
        });
    }

    const form = document.getElementById('themDiaChiForm');
    if (form) {
        // Gắn sự kiện submit cho form thêm địa chỉ
        form.addEventListener('submit', themDiaChi); 
    } else {
        console.error("Lỗi: Không tìm thấy form thêm địa chỉ với ID 'themDiaChiForm'.");
    }
}


// =========================
// 🖼️ Logic tải danh sách địa chỉ (ĐÃ LOẠI BỎ LOGIC MẶC ĐỊNH)
// =========================
async function loadAddresses() {
    const userId = getUserId();
    const addressListDiv = document.getElementById("address-list");

    if (!userId) {
        addressListDiv.innerHTML = `<p class="stext-113 cl6 p-t-10">Vui lòng đăng nhập để xem địa chỉ.</p>`;
        return;
    }
    
    try {
        const res = await fetch(`${diaChiBaseUrl}/NguoiDung/${userId}`); 
        const data = await res.json();
        
        if (res.status !== 200) {
            addressListDiv.innerHTML = `<p class="stext-113 cl6 p-t-10">${data.message || "Không thể tải danh sách địa chỉ."}</p>`;
            return;
        }

        let html = "";
        data.forEach(a => {
            // ĐÃ LOẠI BỎ isDefault
            html += `
                <div class="bor12 p-4 p-lr-30 p-tb-20 m-b-20 address-item" style="border:1px solid #ddd;">

                    <div class="m-b-10">
                        <strong class="cl2">${a.tenNguoiNhan}</strong> – <span>${a.sdt}</span> 
                    </div>
                    
                    <div class="stext-111 cl2 m-b-10">
                        ${a.diaChiCuThe}
                    </div>

                    <div class="action-buttons" style="display:inline-block; margin-top: 10px;">
                        </div>
                </div>
            `;
        });

        addressListDiv.innerHTML = html;
    } catch (error) {
        addressListDiv.innerHTML = `<p class="stext-113 cl6 p-t-10">Lỗi hệ thống khi tải địa chỉ.</p>`;
        console.error(error);
    }
}

// =========================
// ⚡ Khởi chạy (Giữ nguyên)
// =========================
document.addEventListener('DOMContentLoaded', () => {
    // Gọi hàm gắn sự kiện cho việc thêm địa chỉ
    gắnSựKiệnThemDiaChi();
    // Tải danh sách địa chỉ
    loadAddresses();
});

