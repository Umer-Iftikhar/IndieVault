/* --- Carousel Navigation Logic ---*/
document.addEventListener('DOMContentLoaded', function () {
    const navZones = document.querySelectorAll('.iv-nav-zone');

    navZones.forEach(zone => {
        zone.addEventListener('mouseenter', function () {
            // Check if it's the 'prev' (left) or 'next' (right) button
            if (this.classList.contains('carousel-control-prev')) {
                this.classList.add('iv-active-left');
            } else {
                this.classList.add('iv-active-right');
            }
        });

        zone.addEventListener('mouseleave', function () {
            // Remove the shadow classes when mouse leaves
            this.classList.remove('iv-active-left', 'iv-active-right');
        });
    });
});


/* --- AJAX Wishlist --- */
document.addEventListener('DOMContentLoaded', function () {
    // We attach the listener to the document and filter for the ID
    document.addEventListener('click', async function (event) {
        const btn = event.target.closest('#wishlistBtn');
        if (!btn) return;
        event.preventDefault(); // Stop any form submission
        try {
            const gameId = btn.dataset.gameId;
            const isWishlisted = btn.dataset.wishlisted === 'true';
            const token = document.querySelector('input[name="__RequestVerificationToken"]').value;
            // Determine URL based on current state
            const url = isWishlisted ? '/Wishlist/Remove' : '/Wishlist/Add';
            const response = await fetch(url, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': token
                },
                body: JSON.stringify({ gameId: parseInt(gameId) }),
            });
            const result = await response.json();
            if (response.ok && result.success) {
                const newState = !isWishlisted;
                // Update Data Attribute
                btn.dataset.wishlisted = newState.toString();
                // Update UI Classes
                btn.classList.toggle('active', newState);
                // Update Icon
                const icon = btn.querySelector('i');
                if (icon) {
                    icon.className = newState ? 'bi bi-heart-fill me-2' : 'bi bi-heart me-2';
                }
                // Update Text
                const textSpan = btn.querySelector('#wishlistText');
                if (textSpan) {
                    textSpan.textContent = newState ? 'In Wishlist' : 'Add to Wishlist';
                }
            } else {
                console.error("Server error:", result.message);
                alert(result.message || 'Error processing request');
            }
        } catch (error) {
            console.error('Fetch error:', error);
        }
    });
});