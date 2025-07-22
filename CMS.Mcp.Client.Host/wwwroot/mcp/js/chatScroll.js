window.scrollToElement = (element) => {
    if (element instanceof Element) {
        element.scrollIntoView({ behavior: 'smooth', block: 'end' });
    }
};
