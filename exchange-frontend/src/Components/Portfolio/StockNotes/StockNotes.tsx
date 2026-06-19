import { useState } from 'react';
import type { FormEvent } from 'react';
import type { IStockComment } from '../../../Interfaces/APIResponses/IStockComment';
import { useStockComments } from '../../../hooks/useStockComments';
import styles from './StockNotes.module.css';

type Comments = ReturnType<typeof useStockComments>;

const StockNotes = ({ stockId }: { stockId: number }) => {
  const comments = useStockComments(stockId);
  const form = useNoteForm(comments.add);
  return (
    <section className={styles.notes}>
      <NoteForm {...form} />
      <NoteMessage comments={comments} />
      <NoteList comments={comments} />
    </section>
  );
};

function useNoteForm(add: Comments['add']) {
  const [title, setTitle] = useState('');
  const [content, setContent] = useState('');
  const submit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    if (canSubmit(title, content) && await add({ title, content })) {
      setTitle('');
      setContent('');
    }
  };
  return { title, setTitle, content, setContent, submit };
}

function NoteForm(props: NoteFormProps) {
  return (
    <form className={styles.form} onSubmit={props.submit}>
      <input value={props.title} onChange={(event) => props.setTitle(event.target.value)} placeholder="Note title" />
      <textarea value={props.content} onChange={(event) => props.setContent(event.target.value)} placeholder="What matters?" />
      <button type="submit" disabled={!canSubmit(props.title, props.content)}>Save note</button>
    </form>
  );
}

function NoteMessage({ comments }: { comments: Comments }) {
  if (comments.status === 'loading') {
    return <p className={styles.message}>Loading notes...</p>;
  }
  return comments.status === 'error' ? <p className={styles.message}>{comments.message}</p> : null;
}

function NoteList({ comments }: { comments: Comments }) {
  if (comments.values.length === 0 && comments.status !== 'loading') {
    return <p className={styles.empty}>No notes yet.</p>;
  }
  return <ul className={styles.list}>{comments.values.map(renderNote(comments.remove))}</ul>;
}

function renderNote(remove: (id: number) => void) {
  return (comment: IStockComment) => <NoteItem key={comment.id} comment={comment} remove={remove} />;
}

function NoteItem({ comment, remove }: NoteItemProps) {
  return (
    <li className={styles.note}>
      <strong>{comment.title}</strong>
      <span>{formatDate(comment.createdOn)}</span>
      <p>{comment.content}</p>
      <button type="button" onClick={() => remove(comment.id)}>Delete note</button>
    </li>
  );
}

function canSubmit(title: string, content: string) {
  return title.trim().length > 0 && content.trim().length > 0;
}

function formatDate(value: string) {
  return new Date(value).toLocaleDateString();
}

type NoteFormProps = {
  title: string;
  setTitle: (value: string) => void;
  content: string;
  setContent: (value: string) => void;
  submit: (event: FormEvent<HTMLFormElement>) => void;
};

type NoteItemProps = {
  comment: IStockComment;
  remove: (id: number) => void;
};

export default StockNotes;
