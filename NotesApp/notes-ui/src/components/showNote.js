import React from 'react';
import { useState } from 'react';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { faRotate, faTrash } from '@fortawesome/free-solid-svg-icons';
import useNotesApi from '../services/useNotesApi';
import './styles/showNote.css';

const ShowNote = (props) => {
    const {note} = props;
    const [publicLink, setPublicLink] = useState('');

    const notesApi = useNotesApi();

    const generateLink = async() => {
        let data = {
            'resetPublicHashId': false
        };

        let response = await notesApi.generatePublicLink(data, note.hashId);
        if(response.success === true) {
            setPublicLink(`http://localhost:3000/notes/public/${response.data.publicHashId}`);
        }
        else {
            setPublicLink('');
        }
    }

    const deleteLink = async() => {
        let data = {
            'resetPublicHashId': true
        };

        let response = await notesApi.generatePublicLink(data, note.hashId);
        if(response.success === true) {
            setPublicLink('');
        }
    }

    return (
        <div className='note-display'>
            <div className='note-content'>{note.content}</div>
            <div className='link-label'>Generate public link</div>
            <div className='link-container'>
                <input className='link-field' value={publicLink} disabled={true}/>
                <button onClick={generateLink}><FontAwesomeIcon icon={faRotate} /></button>
                <button onClick={deleteLink}><FontAwesomeIcon icon={faTrash} /></button>
            </div>
        </div>
    )
}

export default ShowNote