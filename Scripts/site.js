document.addEventListener("DOMContentLoaded", function () {
    // Set up each row independently
    const ticketRows = [
        { row: "ticketRow1", prevBtn: "prevBtn1", nextBtn: "nextBtn1" },
        { row: "ticketRow2", prevBtn: "prevBtn2", nextBtn: "nextBtn2" }
    ];

    const scrollAmount = 300; // Adjust based on ticket width

    ticketRows.forEach(function (row) {
        const ticketRow = document.getElementById(row.row);
        const prevBtn = document.getElementById(row.prevBtn);
        const nextBtn = document.getElementById(row.nextBtn);

        // Handle "next" button click
        nextBtn.addEventListener("click", function () {
            ticketRow.scrollLeft += scrollAmount;
        });

        // Handle "prev" button click
        prevBtn.addEventListener("click", function () {
            ticketRow.scrollLeft -= scrollAmount;
        });
    });
});