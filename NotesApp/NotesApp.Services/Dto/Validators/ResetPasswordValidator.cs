﻿using FluentValidation;

namespace NotesApp.Services.Dto.Validators
{
    public class ResetPasswordValidator : AbstractValidator<ResetPasswordDto>
    {
        public ResetPasswordValidator()
        {
            RuleFor(x => x.Password)
                .NotEmpty()
                .MinimumLength(6)
                .MaximumLength(20)
                .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9])(?=.*[!@#$%]).{6,20}$");

            RuleFor(x => x.ConfirmPassword).Equal(e => e.Password).WithMessage("Passwords should match");
        }
    }
}
