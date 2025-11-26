// ====================================================
// CẤU HÌNH VÀ HÀM CHUNG
// ====================================================

// ** KHẮC* PHỤC LỖI Chart.js - Khai báo biến global/module để giữ instance Chart *
let monthlyRevenueChartInstance = null;
let inventoryChartInstance = null;
let ratingsChartInstance = null;


// CỔNG API: Kiểm tra lại (đang dùng HTTPS 7109 theo log lỗi cũ)
const API_BASE_URL = 'http://localhost:5150/api/ThongKe'; 

/**
 * Hàm chung để lấy dữ liệu từ API
 * @param {string} endpoint - Tên endpoint (ví dụ: 'TheoThang', 'TongQuan')
 * @param {string} params - Chuỗi query parameters (ví dụ: '?year=2025')
 * @returns {Promise<any>} Dữ liệu JSON hoặc null nếu có lỗi
 */
async function fetchData(endpoint, params = '') {
    try {
        const fullUrl = `${API_BASE_URL}/${endpoint}${params}`;
        const response = await fetch(fullUrl);
        if (!response.ok) {
            const errorText = await response.text();
            console.error(`Lỗi API [${endpoint}] Status: ${response.status}`, errorText);
            throw new Error(`Lỗi khi gọi API: ${endpoint} (Status: ${response.status})`);
        }
        return await response.json();
    } catch (error) {
        console.error(error);
        return null; 
    }
}

// Hàm format tiền tệ
function formatCurrency(amount) {
    // Đảm bảo amount là số và không null
    return (amount || 0).toLocaleString('vi-VN', { style: 'currency', currency: 'VND' });
}

// ====================================================
// LOGIC VẼ BIỂU ĐỒ VÀ RENDER
// ====================================================

// --- 1. Doanh thu theo tháng (Bar Chart) - SỬ DỤNG ENDPOINT /TheoThang MỚI ---
async function renderMonthlyRevenueChart() {
    const selectedYear = document.getElementById('chartMonthlyYearInput')?.value || new Date().getFullYear();
    const chartId = 'monthlyRevenueChart';

    // ** KHẮC PHỤC LỖI CHART.GETCHART **
    if (monthlyRevenueChartInstance) {
        monthlyRevenueChartInstance.destroy();
        monthlyRevenueChartInstance = null;
    }

    const data = await fetchData('TheoThang', `?year=${selectedYear}`); 
    const chartAreaContainer = document.getElementById(chartId)?.parentNode;
    
    if (!chartAreaContainer) return; // Bảo vệ khỏi lỗi null

    if (!data || data.length === 0) {
        chartAreaContainer.innerHTML = `<p class="text-center">Không có dữ liệu doanh thu tháng cho năm ${selectedYear}.</p>`;
        return;
    }
    // Khôi phục lại thẻ canvas nếu đã bị thay thế
    if (!document.getElementById(chartId)) {
        chartAreaContainer.innerHTML = '<canvas id="monthlyRevenueChart"></canvas>';
    }

    const labels = data.map(d => d.thoiGian); 
    const revenues = data.map(d => d.doanhThu);

    const ctx = document.getElementById(chartId)?.getContext('2d');
    if (!ctx) return; 

    // Gán đối tượng Chart mới vào biến instance
    monthlyRevenueChartInstance = new Chart(ctx, {
        type: 'bar',
        data: {
            labels: labels,
            datasets: [{
                label: `Doanh Thu (${selectedYear})`,
                data: revenues,
                backgroundColor: 'rgba(78, 115, 223, 0.8)',
                hoverBackgroundColor: 'rgba(78, 115, 223, 1)',
                borderWidth: 1
            }]
        },
        options: {
            maintainAspectRatio: false,
            responsive: true,
            scales: {
                x: { grid: { display: false } },
                y: {
                    beginAtZero: true,
                    title: { display: true, text: 'Doanh Thu' },
                    ticks: { callback: (value) => formatCurrency(value) }
                }
            },
            plugins: { legend: { display: false } }
        }
    });
}

// --- 2. Tồn kho theo Danh mục ---
async function renderInventoryChart() {
    const data = await fetchData('TonKhoTheoDanhMuc');
    const chartId = 'inventoryChart';
    const chartContainer = document.getElementById(chartId)?.parentNode;
    if (!chartContainer) return;

    if (inventoryChartInstance) {
        inventoryChartInstance.destroy();
        inventoryChartInstance = null;
    }

    if (!data || data.length === 0) {
        chartContainer.innerHTML = '<p class="text-center">Không có dữ liệu tồn kho.</p>';
        return;
    }
    
    // Khôi phục lại thẻ canvas
    if (!document.getElementById(chartId)) {
        chartContainer.innerHTML = '<canvas id="inventoryChart"></canvas>';
    }
    
    const labels = data.map(d => d.tenDanhMuc);
    const inventory = data.map(d => d.tongSoLuongTonKho);

    const ctx = document.getElementById(chartId)?.getContext('2d');
    if (!ctx) return;

    inventoryChartInstance = new Chart(ctx, {
        type: 'bar',
        indexAxis: 'y',
        data: {
            labels: labels,
            datasets: [{
                label: 'Số Lượng Tồn Kho',
                data: inventory,
                backgroundColor: 'rgba(28, 200, 138, 0.8)',
                borderWidth: 1
            }]
        },
        options: {
             indexAxis: 'y',
            maintainAspectRatio: false,
            responsive: true,
            scales: {
                x: { beginAtZero: true, title: { display: true, text: 'Số Lượng' } },
                y: { grid: { display: false } }
            },
            plugins: { legend: { display: false } }
        }
    });
}

// --- 3. Tỷ lệ Đánh giá (Doughnut Chart) ---
async function renderRatingsChart() {
    const data = await fetchData('ThongKeDanhGia');
    const chartId = 'ratingsChart';
    const chartContainer = document.getElementById(chartId)?.parentNode;
    const legendElement = document.getElementById('ratingsLegend');

    if (ratingsChartInstance) {
        ratingsChartInstance.destroy();
        ratingsChartInstance = null;
    }

    if (!data || data.tongSoDanhGia === undefined) {
        if(legendElement) legendElement.innerHTML = '<p class="text-center">Chưa có đánh giá nào.</p>';
        return;
    }

    // Khôi phục lại thẻ canvas
    if (!document.getElementById(chartId) && chartContainer) {
        chartContainer.innerHTML = '<canvas id="ratingsChart"></canvas>';
    }

    const labels = ['5 Sao', '4 Sao', '3 Sao', '2 Sao', '1 Sao'];
    const counts = [
        data.soLuong5Sao || 0, data.soLuong4Sao || 0, data.soLuong3Sao || 0, 
        data.soLuong2Sao || 0, data.soLuong1Sao || 0
    ];

    const ctx = document.getElementById(chartId)?.getContext('2d');
    if (!ctx) return;

    ratingsChartInstance = new Chart(ctx, {
        type: 'doughnut',
        data: {
            labels: labels,
            datasets: [{
                data: counts,
                backgroundColor: ['#1cc88a', '#36b9cc', '#f6c23e', '#e74a3b', '#858796'],
                hoverBorderColor: "rgba(234, 236, 244, 1)",
            }],
        },
        options: {
            maintainAspectRatio: false,
            responsive: true,
            cutout: '80%',
            plugins: {
                legend: { position: 'bottom' },
                tooltip: {
                    callbacks: {
                        label: function(context) {
                            let label = context.label || '';
                            const total = data.tongSoDanhGia;
                            const currentValue = context.raw;
                            const percentage = total === 0 ? '0.0%' : ((currentValue / total) * 100).toFixed(1) + '%';
                            return `${label}: ${currentValue.toLocaleString()} (${percentage})`;
                        }
                    }
                }
            }
        }
    });
}

// --- 4. Trạng thái Đơn hàng (Card) ---
async function renderOrderStatus() {
    const data = await fetchData('TrangThaiDonHang'); 
    const container = document.getElementById('orderStatusCards');
    if (!container) return;

    container.innerHTML = ''; 

    if (!data) {
        container.innerHTML = '<p class="text-danger w-100">Không thể tải trạng thái đơn hàng.</p>';
        return;
    }

    const statuses = [
        { label: 'Đang Xử Lý', count: data.dangXuLy || 0, icon: 'fa-spinner', color: 'primary' },
        { label: 'Đã Xác Nhận', count: data.daXacNhan || 0, icon: 'fa-check-circle', color: 'info' },
        { label: 'Đang Giao', count: data.dangGiao || 0, icon: 'fa-truck', color: 'warning' },
        { label: 'Hoàn Thành', count: data.daGiao || 0, icon: 'fa-box-open', color: 'success' },
        { label: 'Đã Hủy', count: data.daHuy || 0, icon: 'fa-times-circle', color: 'danger' },
    ];

    statuses.forEach(status => {
        const html = `
            <div class="col-md-4 col-sm-6 mb-3">
                <div class="card border-left-${status.color} shadow h-100 py-2">
                    <div class="card-body">
                        <div class="row no-gutters align-items-center">
                            <div class="col mr-2">
                                <div class="text-xs font-weight-bold text-${status.color} text-uppercase mb-1">
                                    ${status.label}</div>
                                <div class="h5 mb-0 font-weight-bold text-gray-800">${status.count.toLocaleString()}</div>
                            </div>
                            <div class="col-auto">
                                <i class="fas ${status.icon} fa-2x text-gray-300"></i>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        `;
        container.insertAdjacentHTML('beforeend', html);
    });
}


// ====================================================
// SETUP EVENT LISTENERS
// ====================================================

function setupEventListeners() {
    
    // Sử dụng toán tử ?. để tránh lỗi 'Cannot read properties of null (reading 'addEventListener')'
    
    document.getElementById('btnFetchTongQuan')?.addEventListener('click', async () => {
        const startDate = document.getElementById('inputStartDate')?.value;
        const endDate = document.getElementById('inputEndDate')?.value;
        const resultElement = document.getElementById('tongQuanResult');
        if(!resultElement) return;
        resultElement.innerHTML = 'Đang tải...';

        if (!startDate || !endDate) {
            resultElement.innerHTML = '<span class="text-danger">Vui lòng chọn cả Ngày Bắt Đầu và Ngày Kết Thúc.</span>';
            return;
        }

        const data = await fetchData('TongQuan', `?startDate=${startDate}&endDate=${endDate}`);
        
        if (data && data.tongDoanhThu !== undefined) {
            const formattedRevenue = formatCurrency(data.tongDoanhThu);
            resultElement.innerHTML = `
                <span class="text-primary">Doanh thu (${startDate} đến ${endDate}): </span><br>
                <span class="text-dark">${formattedRevenue}</span> <br>
                <span class="text-primary">Số đơn hàng: ${data.soLuongDonHangHoanThanh}</span>
            `;
        } else {
            resultElement.innerHTML = '<span class="text-danger">Không tìm thấy dữ liệu Tổng Quan hoặc lỗi API.</span>';
        }
    });
    
    document.getElementById('btnFetchWeeklyDetail')?.addEventListener('click', async () => {
        const week = document.getElementById('inputWeek')?.value;
        const year = document.getElementById('inputYearForWeek')?.value;
        const resultElement = document.getElementById('weeklyDetailResult');
        if(!resultElement) return;
        resultElement.innerHTML = 'Đang tải...';

        if (!week || !year || week < 1 || week > 53 || year < 2000) {
            resultElement.innerHTML = '<span class="text-danger">Vui lòng nhập Tuần và Năm hợp lệ.</span>';
            return;
        }

        const tuanNam = `${week}/${year}`;
        const data = await fetchData('ChiTietTuan', `?tuanNam=${tuanNam}`);
        
        if (data && data.tongDoanhThu !== undefined) {
            const formattedRevenue = formatCurrency(data.tongDoanhThu);
            resultElement.innerHTML = `
                <span class="text-info">Doanh thu Tuần ${tuanNam} (${data.thoiGian}): </span><br>
                <span class="text-dark">${formattedRevenue}</span> <br>
                <span class="text-info">Số đơn hàng: ${data.soDonHang}</span>
            `;
        } else {
            resultElement.innerHTML = '<span class="text-danger">Không tìm thấy dữ liệu chi tiết tuần hoặc lỗi API.</span>';
        }
    });

    document.getElementById('btnFetchYearlyDetail')?.addEventListener('click', async () => {
        const year = document.getElementById('inputYearForDetail')?.value;
        const resultElement = document.getElementById('yearlyDetailResult');
        if(!resultElement) return;
        resultElement.innerHTML = 'Đang tải...';

        if (!year || year < 2000) {
            resultElement.innerHTML = '<span class="text-danger">Vui lòng nhập Năm hợp lệ.</span>';
            return;
        }

        const data = await fetchData('TheoNamChiTiet', `?year=${year}`);
        
        if (data && data.tongDoanhThu !== undefined) {
            const formattedRevenue = formatCurrency(data.tongDoanhThu);
            resultElement.innerHTML = `
                <span class="text-success">Tổng Doanh thu (${year}): </span><br>
                <span class="text-dark">${formattedRevenue}</span> <br>
                <span class="text-success">Tổng đơn hàng: ${data.soLuongDonHangHoanThanh}</span>
            `;
        } else {
            resultElement.innerHTML = '<span class="text-danger">Không tìm thấy dữ liệu chi tiết năm hoặc lỗi API.</span>';
        }
    });

    document.getElementById('btnUpdateMonthlyChart')?.addEventListener('click', () => {
        renderMonthlyRevenueChart(); 
    });
}

// Khởi chạy tất cả
document.addEventListener('DOMContentLoaded', () => {
    // Gọi hàm render ngay để tải dữ liệu khi trang load
    renderMonthlyRevenueChart(); 
    renderInventoryChart();
    renderRatingsChart();
    renderOrderStatus(); 
    
    setupEventListeners(); 
});