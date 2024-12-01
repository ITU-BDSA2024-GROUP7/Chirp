document.addEventListener('DOMContentLoaded', function () {
    const reactionContainers = document.querySelectorAll('.reactions-container');

    reactionContainers.forEach(container => {
        let touchTimeout;
        let reactionsVisible = false;
        let selectedEmoji = null;
        let isLongPress = false;

        // Loop through each reaction button in the container
        const reactionButtons = container.querySelectorAll('.reactions button');

        // Handle normal click on the reactions (immediate select)
        reactionButtons.forEach(button => {
            button.addEventListener('click', function (e) {
                if (!isLongPress) {
                    // Immediate click behavior after long press
                    return true;
                }
                e.preventDefault();  // Prevent default action during long press
            });
        });

        // Show reactions container on long press of any reaction
        reactionButtons.forEach(button => {
            button.addEventListener('touchstart', function (e) {
                isLongPress = false;
                touchTimeout = setTimeout(() => {
                    isLongPress = true;
                    selectedEmoji = button;  // Mark the emoji as selected
                    button.style.transform = 'scale(1.5)';  // Enlarge the emoji on long press
                    e.preventDefault(); // Prevent default action during long press
                }, 500);  // 500ms for long press
            });

            button.addEventListener('touchend', function (e) {
                clearTimeout(touchTimeout);
                if (isLongPress) {
                    // Long press has been detected, but only if selectedEmoji is set
                    selectedEmoji.click();  // Trigger the click event immediately
                }
                // Reset the long press flag and button styles
                button.style.transform = 'scale(1)';  // Reset the size
                isLongPress = false;
            });

            // Cancel reaction selection if touch is canceled
            button.addEventListener('touchcancel', function (e) {
                clearTimeout(touchTimeout);
                button.style.transform = 'scale(1)';  // Reset the size
                isLongPress = false;
            });
        });

        // Close reactions when clicking anywhere outside the container
        document.addEventListener('click', function (e) {
            if (!container.contains(e.target)) {
                hideReactions(container);
                reactionsVisible = false;
                selectedEmoji = null;
            }
        });
    });
});

function showReactions(container) {
    const reactions = container.querySelector('.reactions');
    reactions.style.opacity = '1';
    reactions.style.visibility = 'visible';
}

function hideReactions(container) {
    const reactions = container.querySelector('.reactions');
    reactions.style.opacity = '0';
    reactions.style.visibility = 'hidden';
}
