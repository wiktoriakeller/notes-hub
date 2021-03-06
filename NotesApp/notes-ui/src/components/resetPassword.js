import React from 'react';
import { useState, useEffect, useContext } from 'react';
import { Link, useNavigate, useParams } from 'react-router-dom';
import InputForm from './inputForm';
import './styles/registerForm.css';
import './styles/forgotPassword.css';
import LoginMessageContext from '../services/loginMessageContext';
import useNotesApi from '../services/useNotesApi';

const passwordRegex = /^(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9])(?=.*[!@#$%]).{6,20}$/;

const ResetPassword = () => {
    const [password, setPassword] = useState('');
    const [isPasswordValid, setIsPasswordValid] = useState(false);
    const [passwordFocus, setPasswordFocus] = useState(false);
    const passwordErrorMsg = 'Password should be 6-20 characters long, must include uppercase and lowercase letters, a number and a special character.';

    const [confirm, setConfirm] = useState('')
    const [isConfirmValid, setIsConfirmValid] = useState(false);
    const [confirmFocus, setConfirmFocus] = useState(false);
    const confirmErrorMsg = 'Passwords should match';

    const [errorMsg, setErrorMsg] = useState([]);
    const [showErrors, setShowErrors] = useState(false)
    const [disableButton, setDisableButton] = useState(false);  

    const navigate = useNavigate();
    const params = useParams();
    const notesApi = useNotesApi();
    const {loginMessage, setLoginMessage, isLoginMsgError, setIsLoginMsgError} = useContext(LoginMessageContext);

    useEffect(() => {
        setIsPasswordValid(passwordRegex.test([password]));
        setIsConfirmValid(password === confirm);
      }, [password, confirm]);

    const handleSubmit = async (e) => {
        e.preventDefault();
        setErrorMsg([]);
        setShowErrors(false);
        setDisableButton(true);

        const token = params.id;
        let data = {
            'password': password,
            'confirmPassword': confirm
        };
    
        let response = await notesApi.resetPassword(data, token);
        if(response.success === true) {
            setLoginMessage('Password has been reset!');
            setIsLoginMsgError(false);
            navigate("/accounts/login");
        }
        else {
            let errorMessages = [];
            for(const [_, value] of Object.entries(response.errors)) {
                errorMessages.push(value);
            }
            setShowErrors(true);
            setErrorMsg(errorMessages);
        }

        setDisableButton(false);
    };

    return (
        <div className='register-form'>
        <form className='inner-form' id='forgot-password-form' onSubmit={handleSubmit}>
            <>
            {errorMsg.map((msg) => {
            return <p className={showErrors ? 'error' : 'hide'}>{msg}</p>;
            })}
            <div className='info-title'>Trouble with logging in?</div><br /><div className='info'>You can enter your new password here.</div>
            <InputForm
                label='Password'
                name='password'
                type='password'
                value={password}
                autoComplete='off'
                errorMessage={passwordErrorMsg}
                isValid={isPasswordValid}
                isFocused={passwordFocus}
                onFocus={() => setPasswordFocus(true)}
                onChange={(e) => setPassword(e.target.value)} />
            <InputForm
                label='Confirm password'
                name='confirm'
                type='password'
                value={confirm}
                autoComplete='off'
                errorMessage={confirmErrorMsg}
                isValid={isConfirmValid}
                isFocused={confirmFocus}
                onFocus={() => setConfirmFocus(true)}
                onChange={(e) => setConfirm(e.target.value)} />
            <button type='submit' disabled={!isPasswordValid || !isConfirmValid || disableButton}>Reset password</button>
            </>
            <p className='account-info'>
            <Link to='/accounts/login'>Back</Link>
            </p>
        </form>
        </div>
    )
}

export default ResetPassword