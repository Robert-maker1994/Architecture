import { describe, it, expect, beforeEach } from '@jest/globals';
import { InMemoryTodoRepository } from './todo.repo';


describe('TodoRepo', () => {
    let inMemoryTodoRepository: InMemoryTodoRepository;

    beforeAll(async () => {
        inMemoryTodoRepository = new InMemoryTodoRepository();
    });


    
    afterEach(async () => {
        await inMemoryTodoRepository.delete("1");
    })
    
    
    it('should save a todo', async () => {
                await inMemoryTodoRepository.create({ text: "Buy milk" })

        const todo = await inMemoryTodoRepository.create({ text: "Buy soup" });
        expect(todo.text).toBe("Buy soup");
        expect(todo.id).toBeDefined();
        expect(todo.createdAt).toBeDefined();
    });
    
    it('should find all todos', async () => {
        await inMemoryTodoRepository.create({ text: "Buy milk" })
        const todos = await inMemoryTodoRepository.findAll();
        expect(todos).toHaveLength(1);

    });

    it("should find a todo by id", async () => {
        await inMemoryTodoRepository.create({ text: "Buy milk" })

        const todo = await inMemoryTodoRepository.findById("2");
        expect(todo?.text).toBe("Buy milk");
    });

     it("should update todo name", async () => {
        await inMemoryTodoRepository.create({ text: "Buy milk" })
       const todos = await inMemoryTodoRepository.update("2", { text: "Buy bread" })
        expect(todos?.text).toBe("Buy bread");
    });

});