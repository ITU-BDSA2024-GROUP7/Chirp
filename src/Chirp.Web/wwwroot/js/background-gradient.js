window.addEventListener('load', function() {
    ResizeBackground();
});

window.addEventListener("resize", function() {
    ResizeBackground();
});

function ResizeBackground(){
    const dayImage = new Image();
    dayImage.src = '../images/Day.png';
    dayImage.onload = function() {
        const bodyHeight = document.body.clientHeight;
        const documentHeight = document.documentElement.clientHeight;
        const dayImageHeight = dayImage.height;
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