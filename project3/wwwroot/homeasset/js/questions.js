// document.addEventListener('DOMContentLoaded', function() {
//     var customizeElement = document.getElementById('customize');
//     if (customizeElement) {
//       // Kết nối với tiny-slider hoặc xử lý logic
//     }
//   });
//   //Sửa từ dòng 1 đến 6

document.addEventListener("DOMContentLoaded", function () {
  // Xử lý mở/đóng Accordion
  document.querySelectorAll(".accordion-header").forEach((header) => {
    header.addEventListener("click", () => {
      const content = header.nextElementSibling; // Lấy nội dung tương ứng
      const point = header.querySelector(".point"); // Lấy nút mở/đóng

      // Chuyển đổi hiển thị nội dung
      if (content.style.display === "block") {
        content.style.display = "none"; // Đóng nội dung
        point.classList.remove("open"); // Xoá trạng thái xoay của point
        console.log("Show content");
      } else {
        // Đóng tất cả nội dung đang mở và đặt lại point
        document
          .querySelectorAll(".accordion-content")
          .forEach((openContent) => {
            openContent.style.display = "none";
            const openPoint =
              openContent.previousElementSibling.querySelector(".point");
            if (openPoint) {
              openPoint.classList.remove("open"); // Đặt lại point
              console.log("Close content");
            }
          });

        // Mở nội dung hiện tại
        content.style.display = "block";
        point.classList.add("open"); // Thêm trạng thái xoay cho point
        console.log("SHow content!!!");
      }
    });
  });

  // Xử lý chuyển đổi bước trong Form
  const stepButtons = document.querySelectorAll(".step-btn");
  const steps = document.querySelectorAll(".form-step");

  stepButtons.forEach((btn, index) => {
    btn.addEventListener("click", () => {
      // Xóa bỏ trạng thái active của tất cả các bước
      stepButtons.forEach((b) => b.classList.remove("active"));
      steps.forEach((step) => step.classList.remove("active"));

      // Đánh dấu bước hiện tại là active
      btn.classList.add("active");
      steps[index].classList.add("active");
    });
  });
});
