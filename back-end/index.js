document.addEventListener('DOMContentLoaded', () => {
    const canvas = document.getElementById("puzzleCanvas");
    const ctx = canvas.getContext("2d");

    //Fill in background colour for the square box
    ctx.fillStyle = '#FCF3BB';
    ctx.fillRect(770, 100, 300, 300);
    
    //Draw the border of the square box
    ctx.beginPath();
    // ctx.shadowColor = "#dd5533";
    // ctx.shadowBlur = 5;
    ctx.lineJoin = "bevel";
    ctx.lineWidth = 5;
    ctx.strokeStyle = "black";
    ctx.strokeRect(770, 100, 300, 300);

    //Draw the 9 columns in the box
    ctx.beginPath();
    ctx.moveTo(870, 100);       //move to coordinate (770,100)
    ctx.lineTo(870, 400);
    ctx.strokeStyle = "black";  // Set the stroke color to black
    ctx.lineWidth = 5;          // Set the line width to 5 pixels
    ctx.stroke();

    ctx.beginPath();
    ctx.moveTo(970, 100);       //move to coordinate (970,100)
    ctx.lineTo(970, 400);
    ctx.strokeStyle = "black";  // Set the stroke color to black
    ctx.lineWidth = 5;          // Set the line width to 5 pixels
    ctx.stroke();

    ctx.beginPath();
    ctx.moveTo(770, 200);       //move to coordinate (770,200)
    ctx.lineTo(1070, 200);
    ctx.strokeStyle = "black";  // Set the stroke color to black
    ctx.lineWidth = 5;          // Set the line width to 5 pixels
    ctx.stroke();

    ctx.beginPath();
    ctx.moveTo(770, 300);       //move to coordinate (770,300)
    ctx.lineTo(1070, 300);
    ctx.strokeStyle = "black";  // Set the stroke color to black
    ctx.lineWidth = 5;          // Set the line width to 5 pixels
    ctx.stroke();

    //Draw 9 mini columns in each columns
    drawBorder(770, 133, 1070, 133, 2);
    drawBorder(770, 166, 1070, 166, 2);
    drawBorder(770, 233, 1070, 233, 2);
    drawBorder(770, 266, 1070, 266, 2);
    drawBorder(770, 333, 1070, 333, 2);
    drawBorder(770, 366, 1070, 366, 2);

    drawBorder(803, 100, 803, 400, 2);
    drawBorder(836, 100, 836, 400, 2);
    drawBorder(903, 100, 903, 400, 2);
    drawBorder(936, 100, 936, 400, 2);
    drawBorder(1003, 100, 1003, 400, 2);
    drawBorder(1036, 100, 1036, 400, 2);


    //Function to draw the mini boxes
    function drawBorder(startX, startY, endX, endY, lineWidth){
        ctx.beginPath();
        ctx.moveTo(startX, startY);       //move to coordinate (770,366)
        ctx.lineTo(endX, endY);
        ctx.strokeStyle = "black";  // Set the stroke color to black
        ctx.lineWidth = lineWidth;          // Set the line width to 5 pixels
        ctx.stroke();
    }
}); 