import { ITodo, ITodoRepository } from '../../domain/interfaces/ITodo';
import todosStore from '../database/inMemoryDB';

export class InMemoryTodoRepository implements ITodoRepository {
    async create(todoData: { text: string }): Promise<ITodo> {
        if (!todoData.text || todoData.text.trim() === '') {
            throw new Error("Todo text cannot be empty.");
        }

        const newTodo: ITodo = {
            id: (todosStore.size + 1).toString(),
            text: todoData.text,
            completed: false,
            createdAt: new Date(),
        };
        todosStore.set(newTodo.id, newTodo);
        return newTodo;
    }

    async findById(id: string): Promise<ITodo | null> {
        const todo = todosStore.get(id);
        return todo || null;
    }

    async findAll(): Promise<ITodo[]> {
        return Array.from(todosStore.values());
    }

    async update(id: string, updates: Partial<Omit<ITodo, 'id' | 'createdAt'>>): Promise<ITodo | null> {
        const todo = todosStore.get(id);
        if (!todo) {
            return null;
        }
        // Ensure text is not updated to empty if provided
        if (updates.text !== undefined && (!updates.text || updates.text.trim() === '')) {
            throw new Error("Todo text cannot be updated to empty.");
        }

        const updatedTodo = { ...todo, ...updates };
        todosStore.set(id, updatedTodo);
        return updatedTodo;
    }

    async delete(id: string): Promise<boolean> {
        return todosStore.delete(id);
    }
}
