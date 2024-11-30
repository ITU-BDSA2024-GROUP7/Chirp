document.addEventListener('DOMContentLoaded', function () {
    const reactionContainers = document.querySelectorAll('.reactions-container');

    reactionContainers.forEach(container => {
        let touchTimeout;
        let reactionsVisible = false;
        let selectedEmoji = null;
        let isLongPress = false;
        
        // Normal tap handling
        container.querySelector('button').addEventListener('click', function(e) {
            if (!isLongPress) {
                // Allow normal tap behavior
                return true;
            }
            e.preventDefault();
        });

        container.addEventListener('touchstart', function (e) {
            isLongPress = false;
            touchTimeout = setTimeout(() => {
                isLongPress = true;
                showReactions(container);
                reactionsVisible = true;
                e.preventDefault(); // Only prevent default on long press
            }, 500);
        });

        container.addEventListener('touchmove', function (e) {
            if (reactionsVisible) {
                e.preventDefault();
                const touch = e.touches[0];
                const reactionButtons = container.querySelectorAll('.reactions button');
                
                // Reset all buttons scale
                reactionButtons.forEach(button => {
                    button.style.transform = 'scale(1)';
                });

                // Find hovered emoji
                reactionButtons.forEach(button => {
                    const rect = button.getBoundingClientRect();
                    if (touch.clientX >= rect.left && touch.clientX <= rect.right &&
                        touch.clientY >= rect.top && touch.clientY <= rect.bottom) {
                        button.style.transform = 'scale(1.5)';
                        selectedEmoji = button;
                    }
                });
            }
        });

        container.addEventListener('touchend', function (e) {
            clearTimeout(touchTimeout);
            if (reactionsVisible) {
                e.preventDefault();
                if (selectedEmoji) {
                    selectedEmoji.click();
                }
                hideReactions(container);
                reactionsVisible = false;
                selectedEmoji = null;
            }
            // Reset long press flag
            isLongPress = false;
        });

        // Cancel reaction selection if touch is canceled
        container.addEventListener('touchcancel', function (e) {
            clearTimeout(touchTimeout);
            if (reactionsVisible) {
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