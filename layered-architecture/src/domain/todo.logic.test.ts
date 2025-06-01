import { DomainValidation } from './todo.logic';

describe('CreateTodo', () => {
    it('should fail the domain validation', () => {
        const validation = new DomainValidation();
        expect(validation.validateText("relax")).toBe(true);
    });

    it("should pass the domain validation",  () => {
        const validation = new DomainValidation();
        expect(validation.validateText("Go shopping")).toBe(false);
    });

});
