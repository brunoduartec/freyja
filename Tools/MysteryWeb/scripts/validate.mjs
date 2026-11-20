import { mkdir, readFile, writeFile } from 'node:fs/promises';
import path from 'node:path';
import { fileURLToPath } from 'node:url';
import { validateCasePaths } from '../src/validation.mjs';

const root = path.resolve(path.dirname(fileURLToPath(import.meta.url)), '..', '..', '..');
const args = process.argv.slice(2);
function option(name, fallback) {
  const index = args.indexOf(name);
  return index >= 0 ? args[index + 1] : fallback;
}
const code = (value) => `\`${value}\``;
function actionText(action) {
  const target = action.interactionId ?? action.characterId ?? action.hypothesisId ?? '';
  const evidence = action.evidenceId ? ` (${action.evidenceId})` : action.evidenceIds ? ` (${action.evidenceIds.join(', ')})` : '';
  return `${action.type}${target ? `:${target}` : ''}${evidence}`;
}
function scenarioSection(item, index) {
  const label = item.lostSources.length ? item.lostSources.join(', ') : 'nenhuma';
  const outcome = item.result.solved ? 'SOLUÇÃO encontrada' : 'FALHA: sem percurso';
  const route = item.result.solved ? item.result.path.map(actionText).join(' → ') || '(conclusão já sustentada no estado inicial)' : '—';
  const evidence = item.result.evidenceIds.length ? item.result.evidenceIds.join(', ') : '—';
  const origins = item.result.origins.length ? item.result.origins.join(', ') : '—';
  const blocked = item.result.blocked.length ? item.result.blocked.map((text) => `  - ${text}`).join('\n') : '  - nenhum';
  return `### ${index + 1}. ${item.title ?? item.scenarioId} (${code(item.scenarioId)}) — fontes perdidas: ${label}\n\n**Abertura do cenário:** ${item.opening ?? 'não informada'}  \n**Resultado:** ${outcome}  \n**Estados visitados:** ${item.result.visited}  \n**Evidências usadas:** ${evidence}  \n**Origens independentes:** ${origins}  \n**Percurso:** ${route}\n\n**Requisitos bloqueados:**\n${blocked}`;
}

const casePath = path.resolve(option('--case', path.join(root, 'Tools/MysteryWeb/data/case.json')));
const maxLosses = Number(option('--all-losses', 2));
const outPath = path.resolve(option('--out', path.join(root, 'Source/Narrative/mvp-arrival/validation-report.md')));
const started = performance.now();
const data = JSON.parse(await readFile(casePath, 'utf8'));
const report = validateCasePaths(data, { maxLosses });
const elapsedMs = performance.now() - started;
const memoryScenario = (data.scenarios ?? []).find((scenario) => scenario.prepareActions?.length && scenario.lostSources?.length);
const memoryResult = memoryScenario ? report.authoredScenarios.find((item) => item.scenarioId === memoryScenario.id)?.result : null;
const memoryValid = Boolean(memoryScenario && memoryResult?.solved);
const withinBudget = elapsedMs <= 5000;
const valid = report.valid && memoryValid && withinBudget;
const parts = [
  `# Relatório de validação narrativa\n\nFonte: ${code(data.id)} em ${code(path.relative(root, casePath))}. Busca por estados alcançáveis usando as mesmas ações da interface.`,
  `## Configurações de perda\n\n${report.scenarios.map(scenarioSection).join('\n\n')}`,
  `## Memória obtida antes da perda\n\n${memoryScenario ? scenarioSection({ ...report.authoredScenarios.find((item) => item.scenarioId === memoryScenario.id), lostSources: memoryScenario.lostSources }, 1) : 'FALHA: não há cenário autoral com prepareActions e lostSources para comprovar a memória anterior à perda.'}`,
  `## Resultado\n\n- Caso válido em todas as configurações: **${report.valid ? 'sim' : 'não'}**\n- Percurso de memória anterior válido: **${memoryValid ? 'sim' : 'não'}**\n- Tempo medido: **${elapsedMs.toFixed(1)} ms** (limite: 5.000 ms; ${withinBudget ? 'dentro' : 'acima'} do limite)\n- Configurações de perda examinadas: **${report.scenarios.length}**\n\nA busca verifica alcançabilidade e rastreabilidade de evidências. Não simula o relógio de campanha, a causa das mortes ou a qualidade narrativa; esses pontos exigem revisão humana.`,
];
await mkdir(path.dirname(outPath), { recursive: true });
await writeFile(outPath, parts.join('\n\n---\n\n') + '\n');
process.stdout.write(`Validação ${valid ? 'aprovada' : 'reprovada'}: ${report.scenarios.length} configurações, ${elapsedMs.toFixed(1)} ms.\nRelatório: ${outPath}\n`);
if (!valid) process.exitCode = 1;
