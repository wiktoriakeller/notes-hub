import React, { useState, useEffect } from 'react';
import InputForm from './inputForm';
import Tags from './tags';
import './styles/addNote.css';
import * as validation from '../services/noteValidation.js';

const EditNote = (props) => {
    const { setIsValidForm, setEditFormData, note, allNotes, errorMsg, setErrorMsg, tagsCopy} = props;

    const [name, setName] = useState(note.noteName);
    const [isNameValid, setIsNameValid] = useState(true);
    const [nameFocus, setNameFocus] = useState(false);

    const [imageLink, setImageLink] = useState(note.imageLink);
    const [isImageLinkValid, setIsImageLinkValid] = useState(true);
    const [imageLinkFocus, setImageLinkFocus] = useState(false);

    const [content, setContent] = useState(note.content);
    const [isContentValid, setIsContentValid] = useState(true);
    const [contentFocus, setContentFocus] = useState(false);

    const [tagInput, setTagInput] = useState('');
    const [tagFocus, setTagFocus] = useState(false);
    const [isTagValid, setIsTagValid] = useState(true);
    const [tags, setTags] = useState(tagsCopy);
    
    useEffect(() => {
        let validName = validation.validateName(name, note.hashId, allNotes);
        let validContent = validation.validateContent(content);
        let validLink = validation.validateImageLink(imageLink);
        let validTag = validation.validateTag(tagInput, tags);
        let validForm = validName && validContent && validLink && validTag;

        setIsNameValid(validName);
        setIsContentValid(validContent);
        setIsImageLinkValid(validLink);
        setIsTagValid(validTag);
        setIsValidForm(validForm);

        if(validForm) {
            let data = {
                'noteName': name,
                'content': content,
                'imageLink': imageLink,
                'tags': []
            }

            for(const tag of tags) {
                data.tags.push({'tagName': tag});
            }
            setEditFormData(data);
        }

        setErrorMsg([]);
    }, [name, content, tagInput, imageLink]);

    return (
        <form className='form-container'>
            {errorMsg.map((msg) => {
                return <p className={msg.length > 0 ? 'error' : 'hide'}>{msg}</p>;
            })}
            <span className='not-visible'>Form for editing information about note content and the title.</span>
            <InputForm
                label='Name'
                name='name'
                type='text'
                value={name}
                autoComplete='off'
                errorMessage={validation.nameErrorMsg}
                isValid={isNameValid}
                isFocused={nameFocus}
                maxLength={40}
                onChange={(e) => setName(e.target.value)}
                onFocus={() => setNameFocus(true)}
            />
            
            <InputForm
                label='Image link'
                name='image'
                type='text'
                value={imageLink}
                autoComplete='off'
                errorMessage={validation.imageLinkErrorMsg}
                isValid={isImageLinkValid}
                isFocused={imageLinkFocus}
                onChange={(e) => setImageLink(e.target.value)}
                onFocus={() => setImageLinkFocus(true)}
            />

            <div className='content-field'>
                <label className='input-form-label' htmlFor='content'>Content</label>
                <textarea 
                    id='content' 
                    maxLength={1000} 
                    rows={12} 
                    required
                    value={content}
                    onChange={(e) => setContent(e.target.value)}
                    onFocus={() => setContentFocus(true)}
                    className={isContentValid && contentFocus ? 'textarea-valid' : contentFocus ? 'textarea-invalid' : 'textarea-normal'}
                    >
                </textarea>
                <span className={!isContentValid && contentFocus ? 'error-msg' : 'hide-error-msg'}>{validation.contentErrorMsg}</span>
            </div>

            <Tags
                tags={tags}
                setTags={setTags}
                tagInput={tagInput}
                setTagInput={setTagInput}
                isTagValid={isTagValid}
                tagFocus={tagFocus}
                allowDeleting={true}
                onChange={(e) => setTagInput(e.target.value)}
                onFocus={() => setTagFocus(true)}
            />
        </form>
    )
}

export default EditNote