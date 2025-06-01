export interface ITodo {
  id: string;
  text: string;
  completed: boolean;
  createdAt: Date;
}

export interface ITodoRepository {
  create(todoData: { text: string }): Promise<ITodo>;
  findById(id: string): Promise<ITodo | null>;
  findAll(): Promise<ITodo[]>;
  update(id: string, updates: Partial<Omit<ITodo, 'id' | 'createdAt'>>): Promise<ITodo | null>;
  delete(id: string): Promise<boolean>;
}