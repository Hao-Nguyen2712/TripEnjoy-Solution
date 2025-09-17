using FluentValidation;
using System.Threading;
using System.Threading.Tasks;
using TripEnjoy.Application.Interfaces.Persistence;

namespace TripEnjoy.Application.Features.PropertyType.Commands;

public class CreatePropertyTypeCommandValidator : AbstractValidator<CreatePropertyTypeCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreatePropertyTypeCommandValidator(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Property type name is required.")
            .MaximumLength(200).WithMessage("Property type name must not exceed 200 characters.")
            .MustAsync(BeUniqueName).WithMessage("A property type with this name already exists.");
    }

    private async Task<bool> BeUniqueName(string name, CancellationToken cancellationToken)
    {
        var existingPropertyType = await _unitOfWork.Repository<Domain.PropertyType.PropertyType>()
            .GetAsync(pt => pt.Name.ToLower() == name.ToLower());
        
        return existingPropertyType == null;
    }
}
