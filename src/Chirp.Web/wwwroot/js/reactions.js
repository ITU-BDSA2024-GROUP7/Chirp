document.addEventListener('DOMContentLoaded', function () {
    const reactionContainers = document.querySelectorAll('.reactions-container');

    function isMobile() {
        return window.innerWidth <= 768; // Adjust breakpoint as needed
    }

    reactionContainers.forEach(container => {
        let touchTimeout;
        let reactionsVisible = false;
        let selectedEmoji = null;
        let isLongPress = false;

        const likeButton = container.querySelector('.like-button');
        const reactions = container.querySelector('.reactions');
        const reactionButtons = container.querySelectorAll('.reactions button');

        function showReactions() {
            if (isMobile()) {
                reactions.style.opacity = '1';
                reactions.style.visibility = 'visible';
                reactionsVisible = true;
            }
        }

        function hideReactions() {
            if (isMobile()) {
                reactions.style.opacity = '0';
                reactions.style.visibility = 'hidden';
                reactionsVisible = false;
            }
        }

        // Long press handling for like button
        likeButton.addEventListener('touchstart', function (e) {
            isLongPress = false;
            touchTimeout = setTimeout(() => {
                isLongPress = true;

                // If reactions are not visible, show them
                if (!reactionsVisible) {
                    showReactions();
                }

                e.preventDefault();
            }, 500); // Long press duration
        });

        likeButton.addEventListener('touchend', function () {
            clearTimeout(touchTimeout);

            // If it was a long press and reactions are visible, hide them
            if (isLongPress && reactionsVisible) {
                hideReactions();
            }

            isLongPress = false;
        });

        likeButton.addEventListener('touchcancel', function () {
            clearTimeout(touchTimeout);
            isLongPress = false;
        });

        // Emoji selection handling
        reactionButtons.forEach(button => {
            button.addEventListener('click', function () {
                selectedEmoji = this;
                button.style.transform = 'scale(1.5)';

                // Optional: Add any additional logic for emoji selection
                hideReactions();
            });
        });

        // Hide reactions when clicking outside
        document.addEventListener('click', function (e) {
            if (isMobile() && reactionsVisible &&
                !container.contains(e.target) &&
                !reactions.contains(e.target)) {
                hideReactions();
            }
        });
    });

    // Resize event listener to handle view changes
    window.addEventListener('resize', function () {
        const currentIsMobile = isMobile();

        reactionContainers.forEach(container => {
            const reactions = container.querySelector('.reactions');

            if (!currentIsMobile) {
                // Reset styles for desktop
                if (reactions) {
                    reactions.style.opacity = '';
                    reactions.style.visibility = '';
                }
            }
        });
    });
});