export const nameErrorMsg = "Name should be unique with at least 3 characters.";
export const imageLinkErrorMsg = "Link should lead to an image.";
export const contentErrorMsg = "Content is required.";
export const tagErrorMsg = "Tag should be unique.";

export function validateName(name, noteHashId, notes) {
    let trimmedName = name.toLowerCase().trim();
    let isUnique = true;
    for(const note of notes) {
        if(note.noteName.trim().toLowerCase() === trimmedName && note.hashId !== noteHashId) {
            isUnique = false;
            break;
        }
    }
    
    let isValid = isUnique && trimmedName.length > 2;
    return isValid;
}

export function validateContent(content) {
    let isValid = content !== '';
    return isValid;
}

export function validateTag(tag, tags) {
    let trimmedTag = tag.trim();
    let unique = trimmedTag === '' || !tags.includes(trimmedTag);
    return unique;
}

export function validateImageLink(imageLink) {
    let isValid = false;
    if(imageLink === '' || imageLink === null) {
        isValid = true;
    }
    else if((imageLink.endsWith('.jpg') || imageLink.endsWith('.png') || imageLink.endsWith('.jpeg'))
        && (imageLink.startsWith('https://') || imageLink.startsWith('http://'))) {
        isValid = true;
    }
    return isValid;
}
