window.addEventListener('load', function() {
    ResizeBackground();
});

window.addEventListener("resize", function() {
    ResizeBackground();
});


// Calculation inspired by https://stackoverflow.com/questions/76476276/how-to-get-the-aspect-ratio-of-an-image-using-javascript
// and https://stackoverflow.com/questions/74472466/how-to-get-the-actual-dimensions-of-the-element-displayed-in-viewport
function ResizeBackground(){
    const dayImage = new Image();
    dayImage.src = '../images/Day.png';
    dayImage.onload = function() {
        document.body.appendChild(dayImage); // Temporarily add the image to the DOM to get its rendered size
        const actualDayImageHeight = dayImage.getBoundingClientRect().height;
        const actualDayImageWidth = dayImage.getBoundingClientRect().width;
        document.body.removeChild(dayImage); // Remove the image after getting its size

        const bodyHeight = document.body.clientHeight+10;
        const bodyWidth = document.body.clientWidth;
        const aspectRatio = actualDayImageWidth / actualDayImageHeight;
        const dayImageHeight = bodyWidth / aspectRatio;
        const remainingHeight = bodyHeight - dayImageHeight;
        const dayImageHeightPercentage = (remainingHeight / bodyHeight) * 100;
        if (dayImageHeightPercentage < 0) {
            document.documentElement.style.setProperty('--day-gradient-height', `0%`);
            return;
        } else {
            document.documentElement.style.setProperty('--day-gradient-height', `${dayImageHeightPercentage}%`);
        }
    };
}