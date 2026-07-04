import fs from 'node:fs';
import path from 'node:path';
import { fileURLToPath } from 'node:url';
import ts from 'typescript';

const maxLines = 15;
const scriptRoot = path.dirname(fileURLToPath(import.meta.url));
const frontendRoot = path.resolve(scriptRoot, '..');
const workspaceRoot = path.resolve(frontendRoot, '..');
const ignoredDirectories = new Set(['bin', 'dist', 'node_modules', 'obj', 'Platforms']);

main();

function main() {
  const findings = [...scanFrontendProject(), ...scanCSharpProject()];
  if (findings.length === 0) {
    console.log(`No functions exceed ${maxLines} lines.`);
    return;
  }
  findings.map(formatFinding).forEach((finding) => console.log(finding));
  process.exitCode = 1;
}

function scanFrontendProject() {
  return frontendRoots().flatMap((root) => filesFrom(root, isFrontendCodeFile)).flatMap(scanFrontendFile);
}

function frontendRoots() {
  return [
    path.join(frontendRoot, 'src'),
    path.join(frontendRoot, 'scripts'),
    path.join(frontendRoot, 'vite.config.ts'),
    path.join(frontendRoot, 'eslint.config.js'),
  ];
}

function scanCSharpProject() {
  return cSharpRoots().flatMap((root) => filesUnder(root, isCSharpFile)).flatMap(scanCSharpFile);
}

// Both C# projects follow the same 15-line rule. Platforms/ heads are template-generated, so they are skipped.
function cSharpRoots() {
  return [path.join(workspaceRoot, 'api'), path.join(workspaceRoot, 'Regulas.MauiApp')];
}

function filesFrom(root, predicate) {
  const stat = fs.statSync(root);
  return stat.isDirectory() ? filesUnder(root, predicate) : matchingFile(root, predicate);
}

function matchingFile(file, predicate) {
  return predicate(file) ? [file] : [];
}

function filesUnder(root, predicate) {
  return fs.readdirSync(root, { withFileTypes: true }).flatMap((entry) => {
    const fullPath = path.join(root, entry.name);
    if (entry.isDirectory()) {
      return ignoredDirectory(entry.name) ? [] : filesUnder(fullPath, predicate);
    }
    return predicate(fullPath) ? [fullPath] : [];
  });
}

function ignoredDirectory(name) {
  return ignoredDirectories.has(name);
}

function scanFrontendFile(file) {
  const source = createSourceFile(file);
  const findings = [];
  inspectFrontendNode(source, source, file, findings);
  return findings;
}

function createSourceFile(file) {
  const text = fs.readFileSync(file, 'utf8');
  return ts.createSourceFile(file, text, ts.ScriptTarget.Latest, true, scriptKind(file));
}

function inspectFrontendNode(node, source, file, findings) {
  if (isFunctionLike(node)) {
    recordFrontendNode(node, source, file, findings);
  }
  ts.forEachChild(node, (child) => inspectFrontendNode(child, source, file, findings));
}

function recordFrontendNode(node, source, file, findings) {
  const start = sourceLine(source, node.getStart(source));
  const end = sourceLine(source, node.getEnd());
  const lines = end - start + 1;
  if (lines > maxLines) {
    findings.push(finding(file, functionName(node), start, lines));
  }
}

function scanCSharpFile(file) {
  const lines = fs.readFileSync(file, 'utf8').split(/\r?\n/);
  return lines.flatMap((_, index) => cSharpFinding(file, lines, index));
}

function cSharpFinding(file, lines, index) {
  if (!isMethodStart(lines[index])) {
    return [];
  }
  const end = blockEnd(lines, index);
  return cSharpMethodFinding(file, index, end);
}

function cSharpMethodFinding(file, start, end) {
  const lines = end === null ? 0 : end - start + 1;
  return lines > maxLines ? [finding(file, 'method', start + 1, lines)] : [];
}

function blockEnd(lines, start) {
  let depth = 0;
  let seenBody = false;
  for (let index = start; index < lines.length; index += 1) {
    depth += braceCount(lines[index], '{');
    seenBody = seenBody || depth > 0;
    depth -= braceCount(lines[index], '}');
    if (seenBody && depth === 0) {
      return index;
    }
  }
  return null;
}

function isFunctionLike(node) {
  return ts.isFunctionDeclaration(node)
    || ts.isFunctionExpression(node)
    || ts.isArrowFunction(node)
    || ts.isMethodDeclaration(node);
}

function functionName(node) {
  if (node.name?.text) {
    return node.name.text;
  }
  return node.parent?.name?.text ?? '<anonymous>';
}

function sourceLine(source, position) {
  return source.getLineAndCharacterOfPosition(position).line + 1;
}

function scriptKind(file) {
  if (file.endsWith('.tsx')) {
    return ts.ScriptKind.TSX;
  }
  return file.endsWith('.ts') ? ts.ScriptKind.TS : ts.ScriptKind.JS;
}

function isFrontendCodeFile(file) {
  return /\.(tsx?|[cm]?js)$/.test(file);
}

function isCSharpFile(file) {
  return file.endsWith('.cs');
}

function isMethodStart(line) {
  return methodDeclaration(line) && !isTypeDeclaration(line) && !isExpressionBodied(line);
}

// Single-line expression-bodied members (=> ...;) are one line by definition
// and have no brace block for the scanner to measure.
function isExpressionBodied(line) {
  return /=>.*;\s*$/.test(line);
}

function methodDeclaration(line) {
  return /^\s*(public|private|internal|protected)\s+(static\s+)?(override\s+)?(async\s+)?[\w<>?[\],\s]+\s+\w+\s*\(/.test(line);
}

// Positional records look like methods to the regex above, but type
// declarations are not functions and should not be length-checked.
function isTypeDeclaration(line) {
  return /\b(class|record|struct|interface|enum)\b/.test(line);
}

function braceCount(line, brace) {
  return [...line].filter((character) => character === brace).length;
}

function finding(file, name, start, lines) {
  return { file: label(file), name, start, lines };
}

function label(file) {
  return path.relative(workspaceRoot, file).replaceAll(path.sep, '/');
}

function formatFinding({ file, name, start, lines }) {
  return `${file}:${start} ${name} has ${lines} lines.`;
}
