// File: myaccount/thongtincanhan.js (Ví dụ)

// =========================
// ⚙️ Cấu hình API
// =========================
const taiKhoanBaseUrl = 'http://localhost:5150/api/TaiKhoan'; 

// =========================
// 🔧 Hàm tiện ích (Sử dụng Fetch API)
// =========================
async function fetchJson(url, options = {}) {
    const res = await fetch(url, options);
    const text = await res.text();
    if (!res.ok) {
        let message = text;
        try {
            // Cố gắng parse JSON để lấy thông báo lỗi chi tiết từ Backend
            const json = JSON.parse(text);
            message = json.message || JSON.stringify(json);
        } catch {}
        // Ném ra lỗi với mã trạng thái (ví dụ: 404, 401)
        throw new Error(`[${res.status} ${res.statusText}] ${message}`);
    }
    // Trả về đối tượng JSON (hoặc đối tượng rỗng nếu không có nội dung)
    return text ? JSON.parse(text) : {};
}

// =========================
// 🔔 Hàm showToast (Sử dụng jQuery và Bootstrap Toast)
// =========================
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

// =========================
// 🔑 Hàm lấy thông tin người dùng (GIẢ ĐỊNH)
// =========================
function getUserId() {
    const userInfo = JSON.parse(localStorage.getItem("currentUser"));
    const userId = userInfo.id;
    return userId;
}

function getAuthToken() {
    // Chỉ để chuỗi Token (bao gồm "Bearer " và token JWT)
    return 'Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...'; 
}

// HOẶC 
function getAuthToken() {
    // Chỉ để chuỗi Token, không có bất kỳ ký tự tiếng Việt nào trong chuỗi này
    return 'Bearer ' + 'TOKEN_THUC_TE_CUA_BAN'; 
}

// =========================
// 👤 Load thông tin cá nhân (GET)
// =========================
async function loadThongTinCaNhan() {
    const userId = getUserId();
    const token = getAuthToken();
    const url = `${taiKhoanBaseUrl}/${userId}`;

    console.log(`[DEBUG] Đang gọi API GET: ${url}`);

    try {
        const data = await fetchJson(url, {
            method: 'GET',
            headers: {
                'Authorization': token,
                'Content-Type': 'application/json'
            }
        });
        
        // =======================================================
        // ĐIỀU CHỈNH: Bắt lỗi nếu data trống và dùng toán tử ??
        // =======================================================
        
        console.log("Tải thông tin thành công, Dữ liệu nhận được:", data);

        if (!data || Object.keys(data).length === 0) {
             throw new Error("Không nhận được dữ liệu hồ sơ hợp lệ từ máy chủ. Kiểm tra Network tab.");
        }

        // Tối ưu hóa việc lấy dữ liệu:
        // 1. tenDn (Tên đăng nhập)
        const tenDn = data.tenDn ?? data.TenDn ?? '';
        
        // 2. hoTen (Họ tên)
        const hoTen = data.hoTen ?? data.HoTen ?? ''; 

        // 3. email
        const email = data.email ?? data.Email ?? '';

        // 4. sdt (Số điện thoại)
        const sdt = data.sdt ?? data.Sdt ?? '';

        // Điền dữ liệu vào các trường form
        $("#TenDn").val(tenDn);
        $("#HoTen").val(hoTen);
        $("#Email").val(email);
        $("#Sdt").val(sdt);
        
        showToast("Tải thông tin cá nhân hoàn tất.", "success");

    } catch (err) {
        console.error(`[ERROR] Lỗi tải thông tin: ${err.message}`);
        showToast(`Lỗi tải thông tin: ${err.message}. Kiểm tra Console và Network tab.`, "error");
    }
}

// =========================
// 💾 Cập nhật thông tin cá nhân (PUT)
// =========================
async function updateThongTinCaNhan(event) {
    event.preventDefault(); 
    
    if (typeof $ === 'undefined') {
        return showToast("Lỗi hệ thống: jQuery chưa được tải.", "error");
    }

    const userId = getUserId();
    const token = getAuthToken();
    
    const hoTen = $("#HoTen").val().trim();
    const email = $("#Email").val().trim();
    const sdt = $("#Sdt").val().trim();

    if (!hoTen || !email || !sdt) {
        return showToast("Họ tên, Email và SĐT không được để trống.", "error");
    }
    
    // Payload phải khớp với TaiKhoanUpdateModel trên Backend (PascalCase)
    const payload = { HoTen: hoTen, Email: email, Sdt: sdt };
    
    const url = `${taiKhoanBaseUrl}/${userId}`;
    console.log(`[DEBUG] Đang gọi API PUT: ${url}`);

    try {
        await fetchJson(url, {
            method: 'PUT',
            headers: { 
                'Authorization': token,
                'Content-Type': 'application/json' 
            },
            body: JSON.stringify(payload)
        });

        showToast("Cập nhật thông tin thành công!", "success");
        loadThongTinCaNhan(); 

    } catch (err) {
        console.error(`[ERROR] Lỗi cập nhật: ${err.message}`);
        showToast(`Cập nhật thất bại: ${err.message}`, "error");
    }
}

// =========================
// ⚡ Gắn sự kiện (Sử dụng jQuery ready)
// =========================
$(document).ready(function() {
    // 1. Tải dữ liệu khi trang load
    loadThongTinCaNhan();
    
    // 2. Gán sự kiện submit cho form
    const hoSoForm = $("#hoSoForm");
    if (hoSoForm.length > 0) {
        hoSoForm.on('submit', updateThongTinCaNhan);
    } else {
        console.error("Lỗi: Không tìm thấy form với ID 'hoSoForm'. Không thể gắn sự kiện.");
    }
});